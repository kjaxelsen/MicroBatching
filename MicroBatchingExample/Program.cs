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
    Console.WriteLine(result.ToString());
}