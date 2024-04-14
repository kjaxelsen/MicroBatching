using MicroBatching;
using MicroBatching.Models;
using MicroBatchingExample;

MicroBatchingService service = new MicroBatchingService(new BatchProcessor(), new Logger(), new MicroBatchingServiceOptions()
{
    MaxQueueSize = 10
});
service.StartProcessing();
Console.WriteLine("Waiting 1 second to start");
Thread.Sleep(1000);
List<Task<JobResult>> tasks = new();
for (int i = 1; i <= 20; i++)
{
    Task<JobResult> taskResult = service.AddJobAndProcessAsync(new Job(i, $"test - {i}"));
    tasks.Add(taskResult);
}

service.Shutdown(); // shutdown early but complete preadded jobs

Task<JobResult> failResult = service.AddJobAndProcessAsync(new Job(0, $"test - shouldfail"));
tasks.Add(failResult);

Thread.Sleep(25000);

JobResult[] results = await Task.WhenAll(tasks);
foreach (JobResult result in results)
{
    Console.WriteLine(result.ToString());
}