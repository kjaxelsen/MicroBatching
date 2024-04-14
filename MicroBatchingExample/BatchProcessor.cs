using MicroBatching;

namespace MicroBatchingExample
{
    public class BatchProcessor : IBatchProcessor
    {
        public List<JobResult> Process(List<Job> jobs)
        {
            Thread.Sleep(1000);
            List<JobResult> results = new();
            for (int i = 0; i < jobs.Count; i++)
            {
                Job job = jobs[i];
                JobResultStatus alternatingStatus = i % 2 == 0 ? JobResultStatus.Success : JobResultStatus.Error;
                results.Add(new JobResult(job.Id, alternatingStatus, alternatingStatus.ToString()));
                Console.WriteLine($"Job processed - Id: {job.Id}, Content: {job.Content}");
            }
            return results;
        }
    }
}
