#pragma warning disable IDE0060 //we include key on purpose so it shows in swagger UI
using Yriclium.LlmApi.Models;
using Yriclium.LlmApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Yriclium.LlmApi.Controllers {
    [ApiController]
    [Route("api")]
    public class LlmApiController : ControllerBase {
        private readonly IHttpContextAccessor    context;
        public LlmApiController(IHttpContextAccessor context) {
            this.context       = context;
            if (this.context.HttpContext == null)
                throw new Exception("Something went wrong creating the ApiController");
            this.context.HttpContext.Response.Headers.AccessControlAllowOrigin = "*";
        }

        [HttpPost("instant/message")]
        public Task<string> SendMessage(
            [FromQuery]    string               key,
            [FromBody]     MessageInput         input, 
            [FromServices] StatelessChatService chatService)
        => chatService.SendAsync(input);

        [HttpPost("job/message")]
        public string SendMessageJob(
            [FromQuery]    string               key,
            [FromBody]     MessageJobInput      input, 
            [FromServices] StatelessChatService chatService, 
            [FromServices] JobService           jobService) 
        => jobService.PerformJob(jobService.SendMessage(chatService, input.Message, input.Webhook));
        public record MessageJobInput{public MessageInput Message {get; set;} = new(); public string? Webhook {get; set;}}

        [HttpGet("job/status")]
        public string JobStatus(
            [FromQuery]    string               key,
            [FromQuery]    string               id, 
            [FromServices] JobService           jobService) 
        => jobService.GetStatus(id).ToString();

        [HttpGet("job/response")]
        public string JobResponse(
            [FromQuery]    string               key,
            [FromQuery]    string               id, 
            [FromServices] JobService           jobService)
        => jobService.GetResponse(id);

        [HttpGet("job/queue")]
        public int JobQueue(
            [FromQuery]    string               key,
            [FromServices] JobService           jobService)
        => jobService.QueueSize();

        [HttpGet("health")]
        public bool Health([FromQuery] string key) => true;

        // An open job serves the same function as an open connection
        // even though in terms of networking, there is no open connection
        // we should still count them here.
        // If there is no open connections and no pending jobs, we could shut down the entire server to save on
        // costs and have some other service determine whether the server needs to be started up again.
        // If you choose this approach, you should be aware instant messaging does not get taken into account
        // and should be refrained from being used.
        [HttpGet("connections")]
        public int Connections(
            [FromQuery]    string                 key,
            [FromServices] JobService             jobService,
            [FromServices] ConnectionStoreService connectionStore
        ) => jobService.QueueSize() + connectionStore.Connections();
    }
}