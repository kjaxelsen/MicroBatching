namespace MicroBatching
{
    public class Job
    {
        public Job(int id, string? content)
        {
            Id = id;
            Content = content;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
