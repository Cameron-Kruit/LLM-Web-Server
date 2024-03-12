using Yriclium.LlmApi.Models;

namespace Yriclium.LlmApi.Services;

public static class PromptOrchestrator {
    public static string ToPrompt(SummarizeInput m) =>
$@"
Summarize the following in max {m.RoughLength} characters:
""{m.TextToSummarize}""
";
}