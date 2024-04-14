using MicroBatching.Models;

namespace MicroBatching.Interfaces
{
    /// <summary>
    /// Interface to be injected to process the jobs
    /// </summary>
    public interface IBatchProcessor
    {
        /// <summary>
        /// Process method to actiont he jobs and return the relevant results
        /// </summary>
        /// <param name="jobs">the jobs to be processed by the batch processor</param>
        /// <returns>the results of each job that was processed</returns>
        public List<JobResult> Process(List<Job> jobs);
    }
}
