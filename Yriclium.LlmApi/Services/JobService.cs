using Yriclium.LlmApi.Models;

namespace Yriclium.LlmApi.Services
{
    public class JobService {
        private readonly IHookService hookService;
        public JobService(IHookService hookService) {
            this.hookService = hookService;
        }

        /// <summary>
        /// <c>Jobs</c> - a container for predefined jobs
        /// </summary>
        public static class Jobs {
            public enum Status {PENDING, QUEUED, COMPLETE, UNKNOWN};
            public interface IJob{
                public string ID {get; set;}
                public Task<string> ExecuteAsync();
            }
            public class BaseJob : IJob {
                public string  ID   {get; set;}
                public string? Hook {get; set;}
                public IChatService chatService;
                public MessageInput input;
                public JobService   jobService;

                public BaseJob(IChatService chatService, MessageInput input, JobService jobService) {
                    ID               = Guid.NewGuid().ToString();
                    this.chatService = chatService;
                    this.input       = input;
                    this.jobService  = jobService;
                }

                public async Task<string> ExecuteAsync() {
                    var message = input;
                    var job     = chatService.SendAsync(message);
                    var result  = await job;
                    if(job.IsCompletedSuccessfully)
                        jobService.SuccessCallback(ID, result, message.Message, Hook);
                    else
                        jobService.FailCallback(ID);
                    return result;
                }
            }
            public class CustomJob<T> : BaseJob where T : IJobInput{
                public CustomJob(IChatService chatService, T input, JobService jobService) : base(
                    chatService, input.ToMessage(), jobService
                ){ }
            }
        }

        private Dictionary<string, Jobs.IJob> jobs         = new(); //all jobs in operation
        private Dictionary<string, Jobs.IJob> queuedJobs   = new(); //jobs waiting for execution
        private Dictionary<string, string>   finishedJobs  = new(); //job id + result
        private bool running = false; // whether jobs in operation are being processed

        #region jobs
        public Jobs.BaseJob SendMessage(IChatService chatService, MessageInput input, string? webook = null) 
        => new(chatService, input, this){ Hook = webook };

        //Example of how a custom job would look
        public Jobs.CustomJob<SummarizeInput> Summarize(
            IChatService   chatService, 
            SummarizeInput input, 
            string?        webook = null
        ) => new(chatService, input, this){ Hook = webook };
        #endregion

        public void SuccessCallback(string jobId, string result, string prompt = "", string? jobHook = null) { //called when job has been fulfilled
            Console.WriteLine("✅ - Completed job: " + jobId);
            hookService.Post(jobId, result, prompt, jobHook);
        }
        public void FailCallback(string jobId) { //called when job has failed
            Console.WriteLine("❌ - Failed job: " + jobId);
        }

        public Jobs.Status GetStatus(string jobId) {
            if(queuedJobs.ContainsKey(jobId))
                return Jobs.Status.QUEUED;
            if(finishedJobs.ContainsKey(jobId))
                return Jobs.Status.COMPLETE;
            if(jobs.ContainsKey(jobId))
                return Jobs.Status.PENDING;
            return Jobs.Status.UNKNOWN;
        }
        public string GetResponse(string jobId) {
            if(finishedJobs.ContainsKey(jobId))
                return finishedJobs[jobId];
            return "";
        }
        public int Queue(string jobId) => //gets position in queue of job with jobId
            jobs      .ContainsKey(jobId) ? jobs      .Keys.ToList().IndexOf(jobId) : 
            queuedJobs.ContainsKey(jobId) ? queuedJobs.Keys.ToList().IndexOf(jobId) + jobs.Count : -1;
        public int QueueSize()
            => queuedJobs.Count + jobs.Count;

        public string PerformJob(Jobs.IJob job) {
            //queue this job
            queuedJobs.Add(job.ID, job);
            Console.WriteLine("📑 Added to job queue: " + job.ID);
            //perform the jobs in the queue if there's no jobs running
            if(!running) {
                running = true;
                Task.Run(() => RunJobs());
            }  
            return job.ID;
        }
        private void LogJobs() => Console.WriteLine("📨 - Jobs in execution: " + jobs.Count + ", Jobs in queue: " + queuedJobs.Count + ", Finished jobs: " + finishedJobs.Count);
        private async Task RunJobs() {
            Console.WriteLine("🏃‍♂️ Running all jobs 🏃‍♂️");
            LogJobs();
            jobs    = queuedJobs.ToDictionary(entry => entry.Key, entry => entry.Value);
            queuedJobs.Clear();
            while (jobs.Count > 0) {
                var mainJob = jobs.First();
                Console.WriteLine("🏁 Started job: " + mainJob.Key);
                LogJobs();
                try {
                    var result = await mainJob.Value.ExecuteAsync();
                    Console.WriteLine("✅ RESULT - " + mainJob.Key + " - " + result);
                    finishedJobs.Add(mainJob.Key, result);
                } catch(Exception e) {
                    Console.WriteLine("❌ failed job - " + mainJob.Key + " - " + e.Message + e.StackTrace);
                }
                jobs.Remove(mainJob.Key);
            }
            if(queuedJobs.Count > 0)
                RunJobs().Start();
            running = false;
            Console.WriteLine("✅ Ran all jobs ✅");
            LogJobs();
        }
    }
}