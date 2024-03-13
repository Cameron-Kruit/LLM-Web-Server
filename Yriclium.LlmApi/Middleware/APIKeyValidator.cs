namespace Yriclium.LlmApi.Middleware;

//TODO: instead of throwing errors, update the http context
public class APIKeyValidator {
    string key;
    public APIKeyValidator(IConfiguration configuration) {
        key = configuration["ApiKey"];
    }

    public T WithApiKey<T>(string ApiKey, T output) {
        if(ApiKey != key)
            throw new UnauthorizedAccessException("API key is needed for this action");
        return output;
    }
    public async Task WithApiKey(string ApiKey, Func<Task> output) {
        if(ApiKey != key)
            throw new UnauthorizedAccessException("API key is needed for this action");
        await output();
    }
}