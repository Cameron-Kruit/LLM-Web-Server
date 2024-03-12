namespace Yriclium.LlmApi.Middleware;

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
}