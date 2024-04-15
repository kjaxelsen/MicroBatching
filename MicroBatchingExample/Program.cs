using MicroBatching;
using MicroBatching.Models;
using MicroBatchingExample;

// create the service and add in the batchprocessor, logger and custom service options
MicroBatchingService service = new MicroBatchingService(new BatchProcessor(), new Logger(), new MicroBatchingServiceOptions(1000, 3, null));

// start processing jobs
service.StartProcessing();

// add jobs
List<Task<JobResult>> tasks = new();
for (int i = 1; i <= 20; i++)
{
    // adds a job while returning a task containing the result (when complete)
    Task<JobResult> taskResult = service.AddJobAndProcessAsync(new Job(i, $"test - {i}"));
    tasks.Add(taskResult);
}

// jobs added after a shutdown fail while ones added before still succeed even if incomplete
//service.Shutdown();
//Task<JobResult> failResult = service.AddJobAndProcessAsync(new Job(0, $"test - shouldfail"));
//tasks.Add(failResult);

// wait for all tasks to finish 
JobResult[] results = await Task.WhenAll(tasks);

//output results
foreach (JobResult result in results)
{
    Console.WriteLine(result.ToString());
}

// shut down the service
service.Shutdown();