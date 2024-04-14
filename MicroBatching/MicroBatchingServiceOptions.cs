namespace MicroBatching
{
    /// <summary>
    /// Service options to control the frequency and batch size for the micro batching service
    /// </summary>
    public class MicroBatchingServiceOptions
    {
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
