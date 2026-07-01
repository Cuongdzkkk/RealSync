using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RealSync.Core.Interfaces.Publishing;

public interface IAITextProvider
{
    string ProviderName { get; }
    Task<StructuredAIOutputResult> GenerateStructuredAsync(
        AITextRequest request,
        CancellationToken cancellationToken);

    Task<string> GenerateTextAsync(
        string prompt,
        CancellationToken cancellationToken);
}

public class AITextRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 2048;
    public string? OriginalDataJson { get; set; }
}

public class StructuredAIOutputResult
{
    public StructuredAIOutput Output { get; set; } = null!;
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
}

public class StructuredAIOutput
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public List<string> Hashtags { get; set; } = new();
    public string CallToAction { get; set; } = string.Empty;
    public List<FactItem> FactsUsed { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class FactItem
{
    public string Field { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
