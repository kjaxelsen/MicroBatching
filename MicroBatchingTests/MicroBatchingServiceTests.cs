using MicroBatching;
using MicroBatching.Interfaces;
using MicroBatching.Models;
using Moq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MicroBatchingTests
{
    public class MicroBatchingServiceTests
    {
        private readonly Mock<ILogger> _logger = new();
        private readonly Mock<IBatchProcessor> _batchProcessor = new();
        private readonly MicroBatchingService _service;
        public MicroBatchingServiceTests()
        {
            _service = new MicroBatchingService(_batchProcessor.Object, _logger.Object, new MicroBatchingServiceOptions());
        }

        [Fact]
        public void NoJobs_NothingProcessed()
        {
            _service.StartProcessing();

            // wait long enough that at least one batch would run
            Thread.Sleep(10000);

            // verify it never ran
            _batchProcessor.Verify(b => b.Process(It.IsAny<List<Job>>()), Times.Never);

            // verify processing started but never did a batch
            _logger.Verify(l => l.Log(LogLevel.Debug, "Started Processing."), Times.Once);
            _logger.Verify(l => l.Log(LogLevel.Debug, "Processing Batch."), Times.Never); 
        }

        [Fact]
        public async void AllJobs_ProcessedSuccessfully_WithNoShutdown()
        {
            MockBatchProcessor();

            _service.StartProcessing();

            var stopwatch = Stopwatch.StartNew();

            // add 20 jobs
            List<Task<JobResult>> tasks = new();
            for (int i = 1; i <= 20; i++)
            {
                Task<JobResult> taskResult = _service.AddJob(new Job(i, $"test - {i}"));
                tasks.Add(taskResult);
            }

            // wait for all tasks to have run
            JobResult[] results = await Task.WhenAll(tasks); 

            stopwatch.Stop();

            // verify it processes 4 batches (each batch is 5 by default, so 20/5 = 4)
            _batchProcessor.Verify(b => b.Process(It.IsAny<List<Job>>()), Times.Exactly(4));

            // assert that it took roughly the expected time (2000ms freq by default, so should take ~8000ms for 4 batches)
            Assert.True(stopwatch.ElapsedMilliseconds > 8000 && stopwatch.ElapsedMilliseconds < 10000);

            // assert that all jobs succeeded 
            Assert.True(results.All(r => r.Status == JobResultStatus.Success));
        }

        [Fact]
        public async void CustomOptions_ChangeProcess_AndProcessTime()
        {
            MockBatchProcessor();

            _service.StartProcessing();

            // have the service run a batch of 10 every second (no queue limit)
            _service.UpdateOptions(new MicroBatchingServiceOptions(1000, 10, null));

            var stopwatch = Stopwatch.StartNew();

            // add 20 jobs
            List<Task<JobResult>> tasks = new();
            for (int i = 1; i <= 20; i++)
            {
                Task<JobResult> taskResult = _service.AddJob(new Job(i, $"test - {i}"));
                tasks.Add(taskResult);
            }

            // wait for all tasks to have run
            JobResult[] results = await Task.WhenAll(tasks);

            stopwatch.Stop();

            // verify it processes 2 batches of 10
            _batchProcessor.Verify(b => b.Process(It.IsAny<List<Job>>()), Times.Exactly(2));

            // assert that it took roughly the expected time (should now take ~2000ms)
            // 20 jobs / 10 per batch = 2 batches. 2 batches at 1000ms each is 2000ms
            Assert.True(stopwatch.ElapsedMilliseconds > 2000 && stopwatch.ElapsedMilliseconds < 3000);

            // confirm all jobs succeeded
            Assert.True(results.All(r => r.Status == JobResultStatus.Success));
        }

        [Fact]
        public async void AddedJobs_ErrorOnFullQueue()
        {
            MockBatchProcessor();

            _service.StartProcessing();

            // have the service run a batch of 10 every second (no queue limit)
            _service.UpdateOptions(new MicroBatchingServiceOptions(2000, 10, 1));

            // add 20 jobs
            List<Task<JobResult>> tasks = new();
            for (int i = 1; i <= 20; i++)
            {
                Task<JobResult> taskResult = _service.AddJob(new Job(i, $"test - {i}"));
                tasks.Add(taskResult);
            }

            // wait for all tasks to have run
            JobResult[] results = await Task.WhenAll(tasks);

            // a single job will be added, which will be processed in 1 batch
            _batchProcessor.Verify(b => b.Process(It.IsAny<List<Job>>()), Times.Exactly(1));

            // one job will succeed while the 19 others fail
            Assert.Equal(1, results.Count(r => r.Status == JobResultStatus.Success));
            Assert.Equal(19, results.Count(r => r.Status == JobResultStatus.Error));
        }

        [Fact]
        public async void ExistingJobs_SucceedAfterShutdown_NewJobs_FailAfterShutdown()
        {
            MockBatchProcessor();

            _service.StartProcessing();

            // add 10 jobs
            List<Task<JobResult>> tasks = new();
            for (int i = 1; i <= 10; i++)
            {
                Task<JobResult> taskResult = _service.AddJob(new Job(i, $"test - {i}"));
                tasks.Add(taskResult);
            }

            // shut down after adding half (10) of the jobs
            _service.Shutdown();

            // add 10 more jobs
            for (int i = 11; i <= 20; i++)
            {
                Task<JobResult> taskResult = _service.AddJob(new Job(i, $"test - {i}"));
                tasks.Add(taskResult);
            }

            // wait for all tasks to have run
            JobResult[] results = await Task.WhenAll(tasks);

            // verify it processes 2 successful batches (each batch is 5 by default)
            // only 2 as the latter 2 would-be batches didn't have jobs added due to shutdown
            _batchProcessor.Verify(b => b.Process(It.IsAny<List<Job>>()), Times.Exactly(2));

            // confirm the first half of the jobs succeeded and the latter half failed
            Assert.Equal(10, results.Count(r => r.Status == JobResultStatus.Success));
            Assert.Equal(10, results.Count(r => r.Status == JobResultStatus.Error));
        }

        [Fact]
        public async void Job_ReturnsArgumentNullException_OnNullValue()
        {
            MockBatchProcessor();

            _service.StartProcessing();

            // have the service run a batch of 10 every second (no queue limit)
            _service.UpdateOptions(new MicroBatchingServiceOptions(2000, 10, 1));

            Task<JobResult> taskResult = _service.AddJob(null);

            // confirm null value returns exception
            await Assert.ThrowsAsync<ArgumentNullException>(() => taskResult);
        }

        [Fact]
        public async void ReturnsExistingJobResult_WhenJobAlreadyAdded()
        {
            MockBatchProcessor();

            _service.StartProcessing();

            // add 2 tasks with the same id
            List<Task<JobResult>> tasks = new();
            Task<JobResult> taskResult = _service.AddJob(new Job(1, $"test - {1}"));
            tasks.Add(taskResult);

            // content is ignored when id is duplicate
            Task<JobResult> taskResult2 = _service.AddJob(new Job(1, $"test - duplicate"));
            tasks.Add(taskResult2);

            // confirm the tasks are the same
            Assert.Equal(taskResult, taskResult2);
        }

        private void MockBatchProcessor()
        {
            _batchProcessor.Setup(b => b.Process(It.IsAny<List<Job>>())).Returns((List<Job> input) =>
            {
                List<JobResult> result = new List<JobResult>();
                foreach (Job job in input)
                    result.Add(new JobResult(job.Id, JobResultStatus.Success, job.Content));
                return result;
            });
        }
    }
}