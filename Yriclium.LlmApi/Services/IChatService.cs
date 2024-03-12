using LLama.Common;
using Yriclium.LlmApi.Models;

namespace Yriclium.LlmApi.Services;

public interface IChatService {
    public Task<string> SendAsync(MessageInput message);
}
