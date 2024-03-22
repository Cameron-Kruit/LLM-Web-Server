using Yriclium.LlmApi.Models;
using Yriclium.LlmApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace Yriclium.LlmApi.Controllers {
    [Route("ws")]
    public class WebSocketController : ControllerBase {
        private readonly HttpContext context;
        public WebSocketController(IHttpContextAccessor context) {
            if (context.HttpContext == null)
                throw new Exception("Something went wrong creating the WebSocketController");
            this.context       = context.HttpContext;
            this.context.Response.Headers.AccessControlAllowOrigin = "*";
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("message")]
        public async Task SendMessageFlow(
            [FromQuery   ] string                 key,
            [FromServices] StatelessChatService   chatService, 
            [FromServices] ConnectionStoreService connectionStore
        ) {
            if (context.WebSockets.IsWebSocketRequest) {
                connectionStore.AddConnection();
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await CommunicateWithLLM(webSocket, chatService);
                connectionStore.RemoveConnection();
            } else
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        //TODO: give connections unique IDs
        private static async Task CommunicateWithLLM(WebSocket webSocket, StatelessChatService chatService) {
            await Send(webSocket, "Open to receiving messages");
            var inBuffer      = new byte[3000];
            await webSocket.ReceiveAsync(
                new ArraySegment<byte>(inBuffer), 
                CancellationToken.None
            );
            var           inString  = Encoding.UTF8.GetString(inBuffer).TrimEnd(new char[] { (char)0 });
            MessageInput? inMessage = JsonConvert.DeserializeObject<MessageInput>(inString);
            while (inMessage != null && !string.IsNullOrEmpty(inMessage.Message)) {
                var message   = await chatService.SendAsync(inMessage);
                await Send(webSocket, JsonConvert.SerializeObject(new MessagePayload(){
                    Message = message,
                    Id      = inMessage.Id
                }));
                inBuffer  = new byte[3000];
                await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(inBuffer), 
                    CancellationToken.None
                );
                inString  = Encoding.UTF8.GetString(inBuffer).TrimEnd(new char[] { (char)0 });
                inMessage = JsonConvert.DeserializeObject<MessageInput>(inString);
            }
            await webSocket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "End of interaction",
                CancellationToken.None
            );
            Console.WriteLine("Closed connection!");
        }

        private static async Task Send(WebSocket webSocket, string message) {
            var outBuffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(
                new ArraySegment<byte>(outBuffer, 0, outBuffer.Length),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }


    }
}