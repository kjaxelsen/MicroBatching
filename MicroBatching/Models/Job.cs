namespace MicroBatching.Models
{
    /// <summary>
    /// A job to be added to the micro batching processor
    /// </summary>
    public class Job
    {
        public Job(int id, string? content)
        {
            Id = id;
            Content = content;
            CreatedAtUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// identifier for the job
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Content to be passed along to the batchprocessor
        /// </summary>
        public string? Content { get; set; }
        /// <summary>
        /// A date the job was created in utc
        /// </summary>
        public DateTime CreatedAtUtc { get; set; }
    }
}
