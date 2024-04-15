namespace MicroBatching.Models
{
    /// <summary>
    /// Service options to control the frequency and batch size for the micro batching service
    /// </summary>
    public class MicroBatchingServiceOptions
    {
        public MicroBatchingServiceOptions() { }

        /// <summary>
        /// Construct new micro batching service options
        /// </summary>
        /// <param name="frequency">The frequency that the micro-batching service checks for jobs, denoted in milliseconds</param>
        /// <param name="batchSize">The maximum batch size for the micro-batching service</param>
        /// <param name="maxQueueSize">Restricts the queue size, when exceeded new jobs will automatically error
        /// no limit set on queue when null</param>
        public MicroBatchingServiceOptions(int frequency, int batchSize, int? maxQueueSize)
        {
            Frequency = frequency;
            BatchSize = batchSize;
            MaxQueueSize = maxQueueSize;    
        }

        /// <summary>
        /// The frequency that the micro-batching service checks for jobs, denoted in milliseconds 
        /// </summary>
        public int Frequency { get; set; } = 2000;
        /// <summary>
        /// The maximum batch size for the micro-batching service
        /// </summary>
        public int BatchSize { get; set; } = 5;
        /// <summary>
        /// Restricts the queue size, when exceeded new jobs will automatically error
        /// no limit set on queue when null
        /// </summary>
        public int? MaxQueueSize { get; set; } = null;
    }
}
