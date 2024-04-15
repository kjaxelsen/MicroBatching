using MicroBatching.Interfaces;
using MicroBatching.Models;

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
                results.Add(new JobResult(job.Id, JobResultStatus.Success, JobResultStatus.Success.ToString()));
                Console.WriteLine($"Job processed - Id: {job.Id}, Content: {job.Content}");
            }
            return results;
        }
    }
}
