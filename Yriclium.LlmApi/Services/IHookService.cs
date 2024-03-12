namespace Yriclium.LlmApi.Services;

public interface IHookService {
    public void Post(string jobId, string jobResult, string prompt, string? hookUrl);
}
