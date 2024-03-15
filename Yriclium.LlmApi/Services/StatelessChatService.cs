using LLama.Common;
using Yriclium.LlmApi.Models;
using System.Text;
using static LLama.LLamaTransforms;
using LLama;

namespace Yriclium.LlmApi.Services
{
    //Direct abstraction of ChatSession. Used instead of text completion for more intuitive prompting.
    public class StatelessChatService : IChatService
    {
        private readonly LLamaContext _context;
        public StatelessChatService(IConfiguration configuration) {
                  var @params = new ModelParams(configuration["ModelPath"]);
            using var weights = LLamaWeights.LoadFromFile(@params);
                     _context = new LLamaContext(weights, @params);
        }
        ChatSession Session() => new ChatSession(new InteractiveExecutor(_context))
                        .WithOutputTransform(new KeywordTextOutputStreamTransform(new string[] { "User:", "Assistant:" }, redundancyLength: 8))
                        .WithHistoryTransform(new HistoryTransform());

        public async Task<string> SendAsync(MessageInput message) {
            // Console.WriteLine("prompt: " + message.Message); //useful for debugging only
            var result = Session().ChatAsync(message.ToChatHistory(), new InferenceParams() { 
                MaxTokens   = message.MaxResponseLength,
                AntiPrompts = new string[] { "User:" } 
            });
            var sb     = new StringBuilder();

            await foreach (var r in result) {
                Console.Write(r);
                sb.Append(r);
            }

            return sb.ToString().Replace("User:", "");
        }
    }
    public class HistoryTransform : DefaultHistoryTransform {
        public override string HistoryToText(ChatHistory history) {
            return base.HistoryToText(history) + "\n Assistant:";
        }

    }
}
