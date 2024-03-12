using Yriclium.LlmApi.Middleware;
using Yriclium.LlmApi.Models;
using Yriclium.LlmApi.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text;
using System.Web;

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

        [Route("message")]
        public async Task SendMessageFlow(
            [FromServices] StatelessChatService   chatService, 
            [FromServices] ConnectionStoreService connectionStore
        ){
            if (context.WebSockets.IsWebSocketRequest) {
                connectionStore.AddConnection();
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await CommunicateWithLLM(webSocket, chatService);
                connectionStore.RemoveConnection();
            }
            else
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        private static async Task CommunicateWithLLM(WebSocket webSocket, StatelessChatService chatService) {
            await Send(webSocket, "Open to receiving messages");
            var open          = true;
            var inBuffer      = new byte[3000];
            await webSocket.ReceiveAsync(
                new ArraySegment<byte>(inBuffer), 
                CancellationToken.None
            );

            while (open) {
                var inMessage = Encoding.UTF8.GetString(inBuffer).TrimEnd(new char[] { (char)0 });;
                Console.WriteLine(inBuffer.Length + " - " + inMessage.Length + " - " + inMessage);

                if(inMessage.StartsWith("close")){
                    open = false;
                    Console.WriteLine("Closing connection...");
                } else {
                    var message = await chatService.SendAsync(new MessageInput() {
                        Message = inMessage
                    });
                    await Send(webSocket, message);

                    inBuffer      = new byte[3000];
                    
                    await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(inBuffer), 
                        CancellationToken.None
                    );
                }
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