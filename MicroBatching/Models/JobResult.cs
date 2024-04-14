﻿namespace MicroBatching.Models
{
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
        }

        public int JobId { get; set; }
        public JobResultStatus Status { get; set; }
        public string? Details { get; set; }

        // Override ToString for debugging purposes
        public override string ToString()
        {
            return $"JobId: {JobId}, Status: {Status}, Details: {Details ?? "NA"}";
        }
    }
}