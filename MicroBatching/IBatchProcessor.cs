namespace MicroBatching
{
    public interface IBatchProcessor
    {
        public List<JobResult> Process(List<Job> jobs);
    }
}
