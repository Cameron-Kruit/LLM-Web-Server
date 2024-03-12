using LLama.Common;
using Yriclium.LlmApi.Services;
namespace Yriclium.LlmApi.Models;

public class HistoryInput {
    public class HistoryItem {
        public HistoryItem(string role, string content) {
            Role    = role;
            Content = content;
        }
        public string Role { get; set; }
        public string Content { get; set; }
    }
}

public class MessageInput {
    public string Message { get; set; } = "";
    public List<HistoryInput.HistoryItem> ToHistoryItems() => new(){new ("User", Message)};
    public ChatHistory ToChatHistory() {
        var h = new ChatHistory();
        h   .Messages
            .AddRange(ToHistoryItems()
            .Select(m => new ChatHistory.Message(Enum.Parse<AuthorRole>(m.Role), m.Content)));
        return h;
    }
}

public interface IJobInput {
    MessageInput ToMessage();
}

//Example of how to use IJobInput
public class SummarizeInput : IJobInput {
    public int    RoughLength     { get; set; } = 200;
    public string TextToSummarize { get; set; } =  "";

    public MessageInput ToMessage() => new(){Message = PromptOrchestrator.ToPrompt(this)};
}