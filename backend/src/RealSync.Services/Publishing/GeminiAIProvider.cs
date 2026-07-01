using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Services.Options;
using RealSync.Shared.Exceptions;
namespace RealSync.Services.Publishing;
public class GeminiAIProvider : IAITextProvider
{
    private readonly HttpClient _httpClient;
    private readonly AIOptions _aiOptions;
    private readonly ILogger<GeminiAIProvider> _logger;
    
    // Round-robin key index (thread-safe)
    private static int _keyIndex = -1;
    private static readonly object _keyLock = new();

    public GeminiAIProvider(
        HttpClient httpClient,
        IOptions<AIOptions> aiOptions,
        ILogger<GeminiAIProvider> logger)
    {
        _httpClient = httpClient;
        _aiOptions = aiOptions.Value;
        _logger = logger;
    }
    public string ProviderName => "Gemini";

    /// <summary>
    /// Lấy API Key tiếp theo theo round-robin.
    /// </summary>
    private string GetNextApiKey()
    {
        var allKeys = _aiOptions.GetAllKeys();
        if (allKeys.Count == 0)
            throw new BusinessException("Gemini API Key chưa được cấu hình.");

        if (allKeys.Count == 1)
            return allKeys[0];

        lock (_keyLock)
        {
            _keyIndex = (_keyIndex + 1) % allKeys.Count;
            return allKeys[_keyIndex];
        }
    }

    public async Task<StructuredAIOutputResult> GenerateStructuredAsync(
        AITextRequest request,
        CancellationToken cancellationToken)
    {
        var allKeys = _aiOptions.GetAllKeys();
        if (allKeys.Count == 0)
            throw new BusinessException("Gemini API Key chưa được cấu hình.");

        var schema = new
        {
            type = "OBJECT",
            properties = new
            {
                title = new { type = "STRING" },
                summary = new { type = "STRING" },
                caption = new { type = "STRING" },
                hashtags = new { type = "ARRAY", items = new { type = "STRING" } },
                callToAction = new { type = "STRING" },
                factsUsed = new
                {
                    type = "ARRAY",
                    items = new
                    {
                        type = "OBJECT",
                        properties = new
                        {
                            field = new { type = "STRING" },
                            value = new { type = "STRING" }
                        },
                        required = new[] { "field", "value" }
                    }
                },
                warnings = new { type = "ARRAY", items = new { type = "STRING" } }
            },
            required = new[] { "title", "summary", "caption", "hashtags", "callToAction", "factsUsed", "warnings" }
        };

        int maxRetries = _aiOptions.MaxRetries;
        string currentModel = request.Model;
        Exception? lastException = null;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            var apiKey = GetNextApiKey();
            
            // On last retry attempt, switch to fallback model
            if (attempt == maxRetries && !string.IsNullOrEmpty(_aiOptions.FallbackModel) && currentModel != _aiOptions.FallbackModel)
            {
                _logger.LogWarning("Switching to fallback model: {FallbackModel} (after {Attempts} retries on {Model})",
                    _aiOptions.FallbackModel, attempt, currentModel);
                currentModel = _aiOptions.FallbackModel;
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{currentModel}:generateContent?key={apiKey}";
            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = request.Prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    responseSchema = schema,
                    maxOutputTokens = request.MaxTokens,
                    temperature = request.Temperature
                }
            };

            _logger.LogInformation("Gemini request: Model={Model}, Attempt={Attempt}/{MaxRetries}, KeyIndex={KeyIdx}",
                currentModel, attempt + 1, maxRetries + 1, _keyIndex);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsJsonAsync(url, body, cancellationToken);
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "HTTP connection error on attempt {Attempt}", attempt + 1);
                if (attempt < maxRetries)
                {
                    var delay = (int)Math.Pow(2, attempt) * 1000;
                    _logger.LogInformation("Retrying in {Delay}ms...", delay);
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }
                throw new BusinessException($"Lỗi kết nối HTTP đến Gemini API sau {maxRetries + 1} lần thử. Chi tiết: {ex.Message}");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini API Error: Status={Status}, Attempt={Attempt}, Body={Body}",
                    response.StatusCode, attempt + 1, errBody);

                // Retryable errors: 429 (Quota), 503 (Overloaded), 500, 502, 504
                if (IsRetryableStatusCode(response.StatusCode))
                {
                    lastException = new BusinessException($"Gemini API lỗi {response.StatusCode}");
                    
                    if (attempt < maxRetries)
                    {
                        // Try to parse retryDelay from Gemini response
                        var retryDelay = ParseRetryDelay(errBody);
                        var backoffDelay = retryDelay > 0 ? retryDelay : (int)Math.Pow(2, attempt) * 1000 + Random.Shared.Next(500, 1500);
                        
                        _logger.LogWarning("Retryable error {Status}. Waiting {Delay}ms before retry {Next}/{Max}...",
                            response.StatusCode, backoffDelay, attempt + 2, maxRetries + 1);
                        await Task.Delay(backoffDelay, cancellationToken);
                        continue;
                    }
                    
                    // All retries exhausted
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        throw new BusinessException($"Gemini API Quota Exceeded sau {maxRetries + 1} lần thử. Vui lòng thêm API Keys từ các project Google khác vào cấu hình 'AI.ApiKeys' để tăng hạn mức.");
                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                        throw new BusinessException($"Gemini API đang quá tải (503) sau {maxRetries + 1} lần thử. Vui lòng thử lại sau vài phút.");
                    throw new BusinessException($"Lỗi từ phía AI Provider ({response.StatusCode}) sau {maxRetries + 1} lần thử: {errBody}");
                }

                // Non-retryable errors
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    throw new BusinessException("Gemini API Key không hợp lệ hoặc bị từ chối truy cập.");
                throw new BusinessException($"Lỗi từ phía AI Provider ({response.StatusCode}): {errBody}");
            }

            // SUCCESS — Parse response
            return await ParseSuccessResponse(response, cancellationToken);
        }

        throw lastException ?? new BusinessException("Gemini API thất bại sau tất cả các lần retry.");
    }

    private async Task<StructuredAIOutputResult> ParseSuccessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var jsonElement = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
        
        // Extract token usage metadata
        int promptTokens = 0;
        int completionTokens = 0;
        if (jsonElement.TryGetProperty("usageMetadata", out var usageEl))
        {
            if (usageEl.TryGetProperty("promptTokenCount", out var pTokens))
                promptTokens = pTokens.GetInt32();
            if (usageEl.TryGetProperty("candidatesTokenCount", out var cTokens))
                completionTokens = cTokens.GetInt32();
        }
        
        // Extract text content containing JSON
        if (!jsonElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            throw new BusinessException("Gemini không trả về bất kỳ kết quả ứng viên nào (Blocked by safety filters or empty response).");

        var candidate = candidates[0];
        if (!candidate.TryGetProperty("content", out var content) || 
            !content.TryGetProperty("parts", out var parts) || 
            parts.GetArrayLength() == 0)
            throw new BusinessException("Kết quả trả về thiếu nội dung text.");

        var text = parts[0].GetProperty("text").GetString();
        if (string.IsNullOrEmpty(text))
            throw new BusinessException("Nội dung phản hồi từ Gemini rỗng.");

        // Clean and escape raw control characters in JSON values
        text = CleanJsonString(text);
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var output = JsonSerializer.Deserialize<StructuredAIOutput>(text, options);
            if (output == null) throw new Exception("Deserialization returned null.");
            return new StructuredAIOutputResult
            {
                Output = output,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse structured output JSON from Gemini. Response text: {Text}", text);
            throw new BusinessException($"Dữ liệu trả về từ AI không khớp cấu trúc JSON yêu cầu. Chi tiết: {ex.Message}");
        }
    }

    private static bool IsRetryableStatusCode(System.Net.HttpStatusCode statusCode)
    {
        return statusCode == System.Net.HttpStatusCode.TooManyRequests        // 429
            || statusCode == System.Net.HttpStatusCode.ServiceUnavailable     // 503
            || statusCode == System.Net.HttpStatusCode.InternalServerError    // 500
            || statusCode == System.Net.HttpStatusCode.BadGateway             // 502
            || statusCode == System.Net.HttpStatusCode.GatewayTimeout;        // 504
    }

    /// <summary>
    /// Parse retryDelay from Gemini error response body (in milliseconds).
    /// </summary>
    private static int ParseRetryDelay(string errorBody)
    {
        try
        {
            var doc = JsonDocument.Parse(errorBody);
            if (doc.RootElement.TryGetProperty("error", out var errorEl) &&
                errorEl.TryGetProperty("details", out var details))
            {
                foreach (var detail in details.EnumerateArray())
                {
                    if (detail.TryGetProperty("retryDelay", out var retryDelayEl))
                    {
                        var delayStr = retryDelayEl.GetString(); // e.g. "57s"
                        if (delayStr != null && delayStr.EndsWith("s"))
                        {
                            if (double.TryParse(delayStr.TrimEnd('s'), System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out var seconds))
                            {
                                // Cap at 60 seconds max wait
                                return (int)(Math.Min(seconds, 60.0) * 1000);
                            }
                        }
                    }
                }
            }
        }
        catch { /* ignore parse errors */ }
        return 0;
    }

    private static string CleanJsonString(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        // 1. Strip markdown code block wraps if present
        text = text.Trim();
        if (text.StartsWith("```"))
        {
            var firstLineEnd = text.IndexOf('\n');
            if (firstLineEnd == -1) firstLineEnd = text.IndexOf('\n');
            if (firstLineEnd != -1)
            {
                text = text.Substring(firstLineEnd + 1);
            }
            if (text.EndsWith("```"))
            {
                text = text.Substring(0, text.Length - 3);
            }
            text = text.Trim();
        }
        // 2. Escape raw control characters inside string values
        var sb = new System.Text.StringBuilder();
        bool inString = false;
        bool isEscaped = false;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '"' && !isEscaped)
            {
                inString = !inString;
                sb.Append(c);
            }
            else if (inString)
            {
                if (c == '\\')
                {
                    isEscaped = !isEscaped;
                    sb.Append(c);
                }
                else
                {
                    isEscaped = false;
                    if (c == '\n')
                    {
                        sb.Append("\\n");
                    }
                    else if (c == '\r')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '\n')
                        {
                            // Let the \n handle it
                        }
                        else
                        {
                            sb.Append("\\n");
                        }
                    }
                    else if (c == '\t')
                    {
                        sb.Append("\\t");
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }
            else
            {
                isEscaped = false;
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
    public async Task<string> GenerateTextAsync(
        string prompt,
        CancellationToken cancellationToken)
    {
        var allKeys = _aiOptions.GetAllKeys();
        if (allKeys.Count == 0)
            throw new BusinessException("Gemini API Key chưa được cấu hình.");

        int maxRetries = _aiOptions.MaxRetries;
        string currentModel = _aiOptions.Model;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            var apiKey = GetNextApiKey();
            
            if (attempt == maxRetries && !string.IsNullOrEmpty(_aiOptions.FallbackModel) && currentModel != _aiOptions.FallbackModel)
            {
                currentModel = _aiOptions.FallbackModel;
                _logger.LogWarning("GenerateText: Switching to fallback model: {FallbackModel}", currentModel);
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{currentModel}:generateContent?key={apiKey}";
            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens = _aiOptions.MaxTokens,
                    temperature = _aiOptions.Temperature
                }
            };

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsJsonAsync(url, body, cancellationToken);
            }
            catch (Exception ex)
            {
                if (attempt < maxRetries)
                {
                    await Task.Delay((int)Math.Pow(2, attempt) * 1000, cancellationToken);
                    continue;
                }
                throw new BusinessException($"Lỗi kết nối HTTP đến Gemini API. Chi tiết: {ex.Message}");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini API Error: Status={Status}, Body={Body}", response.StatusCode, errBody);
                
                if (IsRetryableStatusCode(response.StatusCode) && attempt < maxRetries)
                {
                    var backoffDelay = ParseRetryDelay(errBody);
                    if (backoffDelay <= 0) backoffDelay = (int)Math.Pow(2, attempt) * 1000 + Random.Shared.Next(500, 1500);
                    _logger.LogWarning("GenerateText: Retryable error {Status}. Waiting {Delay}ms...", response.StatusCode, backoffDelay);
                    await Task.Delay(backoffDelay, cancellationToken);
                    continue;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    throw new BusinessException($"Gemini API Quota Exceeded sau {attempt + 1} lần thử. Thêm API Keys vào cấu hình để tăng hạn mức.");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    throw new BusinessException("Gemini API Key không hợp lệ hoặc bị từ chối truy cập.");
                throw new BusinessException($"Lỗi từ phía AI Provider ({response.StatusCode}): {errBody}");
            }

            var jsonElement = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            if (!jsonElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                throw new BusinessException("Gemini không trả về bất kỳ kết quả ứng viên nào (Blocked by safety filters or empty response).");

            var candidate = candidates[0];
            if (!candidate.TryGetProperty("content", out var content) || 
                !content.TryGetProperty("parts", out var parts) || 
                parts.GetArrayLength() == 0)
                throw new BusinessException("Kết quả trả về thiếu nội dung text.");

            var text = parts[0].GetProperty("text").GetString();
            return text ?? string.Empty;
        }

        throw new BusinessException("Gemini API thất bại sau tất cả các lần retry.");
    }
}
