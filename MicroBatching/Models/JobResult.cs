namespace MicroBatching.Models
{
    /// <summary>
    /// status returned from the job result
    /// </summary>
    /// <remarks>
    /// could be extended to be more detailed
    /// </remarks>
    public enum JobResultStatus
    {
        Error,
        Success
    }

    public class JobResult
    {
        public JobResult(int jobId, JobResultStatus status, string? details)
        {
            JobId = jobId;
            Status = status;
            Details = details;
            ProcessedAtUtc = DateTime.UtcNow;
        }
        /// <summary>
        /// The associated JobId (should come from the initialising job)
        /// </summary>
        public int JobId { get; set; }
        /// <summary>
        /// the status of the result (error or success)
        /// </summary>
        public JobResultStatus Status { get; set; }
        /// <summary>
        /// any details provided back from the processor or error messages
        /// </summary>
        public string? Details { get; set; }
        /// <summary>
        /// A date the job was processed in utc
        /// </summary>
        public DateTime ProcessedAtUtc { get; set; }

        // Override ToString for debugging purposes
        public override string ToString()
        {
            return $"JobId: {JobId}, Status: {Status}, Details: {Details ?? "NA"}";
        }
    }
}
