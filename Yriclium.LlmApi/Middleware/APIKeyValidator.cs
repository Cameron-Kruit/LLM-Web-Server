namespace Yriclium.LlmApi.Middleware;

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

public class APIKeyValidator {
    private readonly string      key;

    public APIKeyValidator(IConfiguration configuration) {
        key     = configuration["ApiKey"];
    }

    public T? WithApiKey<T>(HttpContext context, string ApiKey, T output) where T : class {
        if(ApiKey != key) {
            context.Response.StatusCode = 401;
            return null;
        }
        return output;
    }
    public async Task WithApiKey(HttpContext context, string ApiKey, Func<Task> output) {
        if(ApiKey != key) {
            context.Response.StatusCode = 401;
            return;
        }
        await output();
    }
}

public class ApiKeyStore {
    private string ApiKey            {get; set;} = Guid.NewGuid().ToString();
    private string PreviousApiKey    {get; set;} = Guid.NewGuid().ToString();
    private string StoredFingerPrint {get; set;} = FingerPrintPrediction();

    public bool Validate(string key) => key == GetApiKey() || key == PreviousApiKey;

    public string GetApiKey() { //returns a new API Key every hour
        var fingerprint = FingerPrintPrediction();
        if(fingerprint != StoredFingerPrint){
            PreviousApiKey    = ApiKey;
            StoredFingerPrint = fingerprint;
            ApiKey = Guid.NewGuid().ToString();
        }
        return ApiKey;
    }

    private static string FingerPrintPrediction() => //Returns a new fingerprint every hour
        DateTime.UtcNow.Year.ToString() + DateTime.UtcNow.DayOfYear.ToString() + DateTime.UtcNow.Hour.ToString();
}
public class ApiKeyMiddleware {
    private readonly RequestDelegate next;
    private readonly ApiKeyStore     apiKeyStore;
    private readonly IConfiguration  configuration;
    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ApiKeyStore apiKeyStore) {
        this.next          = next;
        this.configuration = configuration;
        this.apiKeyStore   = apiKeyStore;
    }

    public async Task InvokeAsync(HttpContext context) {
        var rawKey       = configuration["ApiKey"];
        var externalAuth = configuration.GetValue<bool?>("ExternalAuth") ?? false;

        if(!string.IsNullOrEmpty(rawKey)) { //just leave the API key empty if you don't wanna use one

             //get key from either header or query param
            var key = context.Request.Headers["X-API-KEY"].ToString();
            if(string.IsNullOrEmpty(key))
                key = context.Request.Query  ["key"      ].ToString();

            if (string.IsNullOrEmpty(key)) {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API key missing");
                return;
            }
            
            //if not using an external auth provider, use the raw key, else use the generated key
            if ((!externalAuth && key != rawKey) || (externalAuth && !apiKeyStore.Validate(key))) {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid API key");
                return;
            }
        }
        await next(context);
    }
}
