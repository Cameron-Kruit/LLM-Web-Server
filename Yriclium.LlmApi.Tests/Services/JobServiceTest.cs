using Yriclium.LlmApi.Services;
using Yriclium.LlmApi.Models;
using Microsoft.Extensions.Configuration;
using LLama.Common;

namespace Yriclium.LlmApi.Tests;

//TODO: maintain these better
public class JobServiceTest
{
    //mocked job
    private class TestJob : JobService.Jobs.Job {
        public string ID { get; set; }
        public int sleepTime;
        public bool done = false;
        public string successResult = "success",
                      result        = "";
        public Task<string> ExecuteAsync() => Task.Run(() => {
                Thread.Sleep(sleepTime);
                done   = true;
                result = successResult;
                //error handling is different per job.
                //if we write our jobs and tests correctly, the jobservice should not have to handle errors
                if(successResult == "error")
                    result = new Exception("Planned exception").ToString(); //act like some error handling has been done
                return result;
            });
    }
    //mocked chat service
    private class TestChatService : IChatService {
        public string LLMResponse = "";
        public Task<string> SendAsync(MessageInput message) => Task.Run(() => {
            if(LLMResponse == "error")
                throw new Exception("Planned Exception");
            return LLMResponse;
        });
    }
    //mocked webhook service
    private class TestHookService : IHookService {
        public Dictionary<string, string> requestsSent = new();
        public void Post(string jobId, string jobResult) {
            requestsSent.Add(jobId, jobResult);
        }
    }
    
    //performing basic job and checking results
    [Fact]
    public void Job_Execution_Test() {
        Console.WriteLine("A job should be executable through the job service.");
        var service = new JobService(new TestHookService());
        var job = new TestJob() {sleepTime = 500};
        Assert.True(service.QueueSize() == 0            , "Queue starts off empty");
        service.PerformJob(job);
        Assert.True(service.QueueSize() == 1            , "Job gets added to queue");
        Thread.Sleep(job.sleepTime + 100); //leave some room for error in the timeout
        Assert.True(job.done                            , "Job has been completed");
        Assert.True(job.result == job.successResult     , "Job has been performed succesfully");
        Assert.True(service.QueueSize() == 0            , "Queue is empty");
    }

    //performing multiple jobs
    [Fact]
    public void Jobs_Execution_Test() {
        Console.WriteLine("Jobs should wait in the queue when other jobs are being performed");
        var service = new JobService(new TestHookService());
        var job1 = new TestJob() { sleepTime = 200, successResult = "Success 1"};
        var job2 = new TestJob() { sleepTime = 200, successResult = "error"};
        var job3 = new TestJob() { sleepTime = 200, successResult = "Success 3"};
        Assert.True(service.QueueSize() == 0            , "Queue starts off empty");
        var jID1 = service.PerformJob(job1);
        var jID2 = service.PerformJob(job2);
        var jID3 = service.PerformJob(job3);
        Assert.True(service.QueueSize() == 3            , "Jobs get added to queue");
        Assert.True(service.Queue(jID1) == 0
                 && service.Queue(jID2) == 1
                 && service.Queue(jID3) == 2            , "Jobs get added in the correct order");
        System.Threading.Thread.Sleep(job1.sleepTime + 100); //leave some room for error in the timeout
        Assert.True(service.Queue(jID1) == -1
                 && service.Queue(jID2) == 0
                 && service.Queue(jID3) == 1            , "Jobs in queue get performed - part 1");
        System.Threading.Thread.Sleep(job2.sleepTime);
        Assert.True(service.Queue(jID1) == -1
                 && service.Queue(jID2) == -1
                 && service.Queue(jID3) == 0            , "Jobs in queue get performed - part 2");
        System.Threading.Thread.Sleep(job3.sleepTime);
        Assert.True(service.Queue(jID1) == -1
                 && service.Queue(jID2) == -1
                 && service.Queue(jID3) == -1           , "Jobs in queue get performed - part 3");
        Assert.True(service.QueueSize() == 0            , "Queue is empty");
        Assert.True(job1.result == job1.successResult
                 && job2.result != job2.successResult
                 && job3.result == job3.successResult   , "Jobs results are the expected results.");
    }

}