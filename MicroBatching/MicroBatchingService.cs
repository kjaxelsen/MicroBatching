using MicroBatching.Interfaces;
using MicroBatching.Models;

namespace MicroBatching
{
    public class MicroBatchingService
    {
        private readonly IBatchProcessor _batchProcessor;
        private readonly ILogger _logger;
        private readonly MicroBatchingServiceOptions _options;
        private readonly Queue<Job> _jobs = new();
        private readonly Dictionary<int, TaskCompletionSource<JobResult>> _results = new();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0); // used to sync job information across threads
        private readonly object _lock = new(); // used to lock content to current thread for thread safety
        private bool _shutdown = false;
        private bool _processing = false;

        /// <summary>
        /// New micro batching service with custom batching options
        /// </summary>
        /// <param name="batchProcessor">implements the IBatchProcessor interface and processes the batched jobs</param>
        /// <param name="logger">simple logger implementing the ILogger interface, sets the logger to receive logs</param>
        /// <param name="options">custom batching options for the service</param>
        public MicroBatchingService(IBatchProcessor batchProcessor, ILogger logger, MicroBatchingServiceOptions options)
        {
            _batchProcessor = batchProcessor;
            _logger = logger;
            _options = options;
        }

        /// <summary>
        /// overrides the existing micro batching service options
        /// </summary>
        /// <param name="newOptions">the new set of options to replace the current ones</param>
        public void UpdateOptions(MicroBatchingServiceOptions newOptions)
        {
            lock (_lock)
            {
                _options.Frequency = newOptions.Frequency;
                _options.BatchSize = newOptions.BatchSize;
                _options.MaxQueueSize = newOptions.MaxQueueSize;
            }
        }

        /// <summary>
        /// stop the micro-batching service after all of the existing jobs have finished processing
        /// </summary>
        public void Shutdown()
        {
            _shutdown = true;
            _logger.Log(LogLevel.Debug, "Shutting down");
        }

        /// <summary>
        /// adds a job to the list of jobs to be batched and processed
        /// </summary>
        /// <param name="job">the job to be added and processed</param>
        /// <returns>a task containing the job result</returns>
        public Task<JobResult> AddJob(Job job)
        {
            if (job == null)
            {
                _logger.Log(LogLevel.Error, "No job provided");
                return Task.FromException<JobResult>(new ArgumentNullException());
            }

            if (_shutdown)
            {
                _logger.Log(LogLevel.Error, $"Job {job.Id} cannot be processed as the service has shut down.");
                return Task.FromResult(ErrorJobResult(job.Id, $"Job {job.Id} cannot be processed as the service has shut down."));
            }

            if (_results.ContainsKey(job.Id))
            {
                _logger.Log(LogLevel.Warning, $"Job {job.Id} has already been added");
                return _results[job.Id].Task;
            }

            if (_options.MaxQueueSize != null && _jobs.Count >= _options.MaxQueueSize)
            {
                _logger.Log(LogLevel.Error, $"Job {job.Id} cannot be added as the queue is full.");
                return Task.FromResult(ErrorJobResult(job.Id, $"Job {job.Id} cannot be added as the queue is full."));
            }


            lock (_lock)
            {
                _jobs.Enqueue(job);
                _semaphore.Release(); // Signal that a job has been added
                var tcs = new TaskCompletionSource<JobResult>();
                _results[job.Id] = tcs;
                return tcs.Task;
            }
        }

        /// <summary>
        /// Begin processing batches
        /// </summary>
        public void StartProcessing()
        {
            Task.Run(BatchProcessingLoop);
        }

        private async Task BatchProcessingLoop()
        {
            if (_processing)
            {
                _logger.Log(LogLevel.Error, "MicroBatchingService is already running.");
                return;
            }

            _logger.Log(LogLevel.Debug, "Started Processing.");

            _processing = true;

            while (!_shutdown || _jobs.Count > 0) // while not shutdown or remaining jobs
            {
                await _semaphore.WaitAsync(); // Wait until there are some jobs for processing

                // Wait for the next execution interval
                await Task.Delay(_options.Frequency);

                _logger.Log(LogLevel.Debug, "Processing Batch.");

                List<Job> batch = new();

                lock (_lock)
                {
                    // Retrieve from queue up to batch size
                    while (_jobs.Count > 0 && batch.Count < _options.BatchSize)
                    {
                        batch.Add(_jobs.Dequeue());
                    }
                }

                // Process the batch jobs
                if (batch.Count > 0)
                {
                    List<JobResult> batchResults = _batchProcessor.Process(batch);
                    lock (_lock)
                    {
                        foreach (JobResult result in batchResults)
                        {
                            // set the pending results into the results table
                            _results[result.JobId].SetResult(result);
                        }
                    }
                }
            }

            _processing = false;
            _logger.Log(LogLevel.Debug, "Finished processing.");
        }

        private JobResult ErrorJobResult(int jobId, string? details)
        {
            return new JobResult
            (
                jobId,
                JobResultStatus.Error,
                details
            );
        }
    }
}
