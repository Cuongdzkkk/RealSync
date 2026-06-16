namespace RealSync.Services.Options;

/// <summary>
/// Cấu hình AI Provider (Gemini, OpenAI,...).
/// </summary>
public class AIOptions
{
    public const string SectionName = "AI";

    /// <summary>Tên provider: "Gemini" hoặc "OpenAI"</summary>
    public string Provider { get; set; } = "Gemini";

    /// <summary>API Key (lấy từ Google AI Studio hoặc OpenAI)</summary>
    public string? ApiKey { get; set; }

    /// <summary>Model name: "gemini-2.0-flash" / "gpt-4o-mini" / ...</summary>
    public string Model { get; set; } = "gemini-2.0-flash";

    /// <summary>Số token tối đa</summary>
    public int MaxTokens { get; set; } = 2048;

    /// <summary>Nhiệt độ sáng tạo (0.0 - 1.0)</summary>
    public double Temperature { get; set; } = 0.8;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiKey);
}
