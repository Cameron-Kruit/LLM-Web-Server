using System.Text;
using Newtonsoft.Json;
namespace Yriclium.LlmApi.Services;

public class ConfigurableWebHookService : IHookService {
    private readonly string? defaultHook;
    private readonly HttpClient client = new();
    public ConfigurableWebHookService(IConfiguration configuration) {
        defaultHook = configuration["Webhook"];
    }
    public async void Post(string jobId, string jobResult, string prompt = "", string? hookUrl = null) {
        var hook = hookUrl ?? defaultHook;
        if(string.IsNullOrEmpty(hook))
            return;

        Console.WriteLine(DateTime.UtcNow.ToString() + " - " + hook);
        var result = client.PostAsync(hook, new StringContent(
            JsonConvert.SerializeObject(new PostPayload(){Id = jobId, Result = jobResult, Prompt = prompt}), 
            Encoding.UTF8, 
            "application/json"));
        Console.WriteLine(DateTime.UtcNow.ToShortTimeString() + " - " + hook + " - " + result.Result.StatusCode + " - " + await result.Result.Content.ReadAsStringAsync() + "\n\n");
    }
    public record PostPayload { public string Id {get; set;} = ""; public string Result {get; set;} = ""; public string Prompt {get; set;} = ""; }
}
