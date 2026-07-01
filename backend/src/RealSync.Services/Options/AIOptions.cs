namespace RealSync.Services.Options;

/// <summary>
/// Cấu hình AI Provider (Gemini, OpenAI,...).
/// </summary>
public class AIOptions
{
    public const string SectionName = "AI";

    /// <summary>Tên provider: "Gemini" hoặc "OpenAI"</summary>
    public string Provider { get; set; } = "Gemini";

    /// <summary>API Key chính (lấy từ Google AI Studio hoặc OpenAI)</summary>
    public string? ApiKey { get; set; }

    /// <summary>Danh sách API Keys để round-robin rotation (mỗi key từ project khác nhau)</summary>
    public List<string>? ApiKeys { get; set; }

    /// <summary>Model name: "gemini-2.5-flash-lite" / "gemini-2.0-flash" / ...</summary>
    public string Model { get; set; } = "gemini-2.0-flash";

    /// <summary>Model dự phòng khi model chính bị quá tải (429/503)</summary>
    public string FallbackModel { get; set; } = "gemini-2.0-flash-lite";

    /// <summary>Số token tối đa</summary>
    public int MaxTokens { get; set; } = 2048;

    /// <summary>Nhiệt độ sáng tạo (0.0 - 1.0)</summary>
    public double Temperature { get; set; } = 0.8;

    /// <summary>Số lần retry tối đa khi gặp lỗi 429/503</summary>
    public int MaxRetries { get; set; } = 3;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiKey) || (ApiKeys != null && ApiKeys.Count > 0);

    /// <summary>Lấy tất cả keys khả dụng (gộp ApiKey đơn + ApiKeys mảng)</summary>
    public List<string> GetAllKeys()
    {
        var keys = new List<string>();
        if (!string.IsNullOrWhiteSpace(ApiKey))
            keys.Add(ApiKey);
        if (ApiKeys != null)
        {
            foreach (var k in ApiKeys)
            {
                if (!string.IsNullOrWhiteSpace(k) && !keys.Contains(k))
                    keys.Add(k);
            }
        }
        return keys;
    }
}
