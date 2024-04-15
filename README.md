# MicroBatchingService
This micro batching service allows the user to submit jobs and have them batched and passed on to a user-injected processor.
The intention is to improve throughput by reducing the number of requests made to a downstream system.

# Usage
## MicroBatchingService
The MicroBatchingService provides the following methods to interact with:

**MicroBatchingService**: The class must be initialised, requires input `IBatchProcessor`, `ILogger`, `MicroBatchingServiceOptions`

**StartProcessing**: Begins the processing (once jobs are available)

**AddJob**: Adds the job to the queue to be processed, returns a task that will contain the JobResult once processing is complete, requires input `Job`

**Shutdown**: Rejects any new jobs and shuts down the service once current jobs in the queue are complete

**UpdateOptions**: Overrides the existing service options with new values, requires `MicroBatchingServiceOptions`

## IBatchProcessor
The IBatchProcessor is an interface that should be implemented by the user, so that the batched jobs can be processed as they choose.
It contains only a single method that takes in a list of jobs and outputs a list of job results.
```csharp
public List<JobResult> Process(List<Job> jobs);
```

## ILogger
The ILogger is a simple logging service to be implemented and injected by the user to receive the relevant logging data
```csharp
public interface ILogger
{
    void Log(LogLevel level, string message);
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}
```

## MicroBatchingServiceOptions
MicroBatchingServiceOptions contain options to determine how data is batched, including:

**Frequency**: The frequency that the micro-batching service checks for jobs, denoted in milliseconds

**BatchSize**: The maximum batch size for the micro-batching service

**MaxQueueSize**: Restricts the queue size, when exceeded new jobs will automatically error - no limit set on queue when null
# Example
## Code
```csharp
using MicroBatching;
using MicroBatching.Models;
using MicroBatchingExample;

// create the service and add in the batchprocessor, logger and custom service options
MicroBatchingService service = new MicroBatchingService(new BatchProcessor(), new Logger(), new MicroBatchingServiceOptions(1000, 3, null));

// start processing jobs
service.StartProcessing();

// modify service options even after processing has started
service.UpdateOptions(new MicroBatchingServiceOptions(2000, 4, 100));

// add jobs
List<Task<JobResult>> tasks = new();
for (int i = 1; i <= 20; i++)
{
    // adds a job while returning a task containing the result (when complete)
    Task<JobResult> taskResult = service.AddJob(new Job(i, $"test - {i}"));
    tasks.Add(taskResult);
}

// shut down the service (waits for already added jobs)
service.Shutdown();

// jobs added after a shutdown fail while ones added before still succeed even if incomplete
Task<JobResult> failResult = service.AddJob(new Job(0, $"test - shouldfail"));
tasks.Add(failResult);

// wait for all tasks to finish 
JobResult[] results = await Task.WhenAll(tasks);

//output results
foreach (JobResult result in results)
{
    // custom tostring to output result for debugging
    Console.WriteLine(result.ToString());
}

// shut down the service
service.Shutdown();
```
## Output
```
JobId: 1, Status: Success, Details: Success
JobId: 2, Status: Success, Details: Success
JobId: 3, Status: Success, Details: Success
JobId: 4, Status: Success, Details: Success
JobId: 5, Status: Success, Details: Success
JobId: 6, Status: Success, Details: Success
JobId: 7, Status: Success, Details: Success
JobId: 8, Status: Success, Details: Success
JobId: 9, Status: Success, Details: Success
JobId: 10, Status: Success, Details: Success
JobId: 11, Status: Success, Details: Success
JobId: 12, Status: Success, Details: Success
JobId: 13, Status: Success, Details: Success
JobId: 14, Status: Success, Details: Success
JobId: 15, Status: Success, Details: Success
JobId: 16, Status: Success, Details: Success
JobId: 17, Status: Success, Details: Success
JobId: 18, Status: Success, Details: Success
JobId: 19, Status: Success, Details: Success
JobId: 20, Status: Success, Details: Success
JobId: 0, Status: Error, Details: Job 0 cannot be processed as the service has shut down.
```
# Tests
To run the tests, run **MicroBatchingTests** in your IDE's unit test explorer
