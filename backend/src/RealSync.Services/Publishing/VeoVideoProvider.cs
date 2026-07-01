using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Services.Options;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Publishing;

public class VeoVideoProvider : IVideoGenerationProvider
{
    private readonly HttpClient _httpClient;
    private readonly VideoOptions _videoOptions;
    private readonly AIOptions _aiOptions;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<VeoVideoProvider> _logger;

    public VeoVideoProvider(
        HttpClient httpClient,
        IOptions<VideoOptions> videoOptions,
        IOptions<AIOptions> aiOptions,
        IHostEnvironment environment,
        ILogger<VeoVideoProvider> logger)
    {
        _httpClient = httpClient;
        _videoOptions = videoOptions.Value;
        _aiOptions = aiOptions.Value;
        _environment = environment;
        _logger = logger;
    }

    public string ProviderName => "Veo";

    public async Task<VideoOperationResult> StartGenerationAsync(
        string prompt,
        string aspectRatio,
        int durationSeconds,
        CancellationToken cancellationToken)
    {
        var apiKey = !string.IsNullOrWhiteSpace(_videoOptions.ApiKey) ? _videoOptions.ApiKey : _aiOptions.ApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new BusinessException("AI Video Provider API Key chưa được cấu hình. Vui lòng thiết lập trong Cài đặt hệ thống.");
        }

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_videoOptions.Model}:generateVideos?key={apiKey}";

        var body = new
        {
            prompt = prompt,
            aspectRatio = aspectRatio,
            durationSeconds = durationSeconds
        };

        _logger.LogInformation("Initiating Veo video generation. Model={Model}, AspectRatio={AspectRatio}, Duration={Duration}s", 
            _videoOptions.Model, aspectRatio, durationSeconds);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync(url, body, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Lỗi kết nối HTTP đến Google Veo API. Chi tiết: {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Veo API Error: Status={Status}, Body={Body}", response.StatusCode, errBody);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                throw new BusinessException("Veo API Quota Exceeded (Lượt truy cập vượt giới hạn định mức).");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                throw new BusinessException("Veo API Key không hợp lệ hoặc bị từ chối truy cập.");

            throw new BusinessException($"Lỗi từ phía AI Video Provider ({response.StatusCode}): {errBody}");
        }

        var jsonElement = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

        if (!jsonElement.TryGetProperty("name", out var nameProperty))
        {
            throw new BusinessException("Không tìm thấy Operation ID (name) trong kết quả phản hồi từ Veo.");
        }

        var operationId = nameProperty.GetString() ?? string.Empty;
        var done = false;
        string? videoUrl = null;

        if (jsonElement.TryGetProperty("done", out var doneProperty))
        {
            done = doneProperty.GetBoolean();
        }

        if (done && jsonElement.TryGetProperty("response", out var responseProperty))
        {
            videoUrl = ExtractVideoUrl(responseProperty);
        }

        return new VideoOperationResult
        {
            OperationId = operationId,
            Done = done,
            VideoUrl = videoUrl
        };
    }

    public async Task<VideoOperationStatusResult> GetOperationStatusAsync(
        string operationId,
        CancellationToken cancellationToken)
    {

        var apiKey = !string.IsNullOrWhiteSpace(_videoOptions.ApiKey) ? _videoOptions.ApiKey : _aiOptions.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new BusinessException("AI Video Provider API Key chưa được cấu hình.");
        }

        // Standard url for operations query:
        // If the operationId returned is of style "operations/12345", we use it, otherwise format
        var namePath = operationId.StartsWith("operations/") ? operationId : $"operations/{operationId}";
        var url = $"https://generativelanguage.googleapis.com/v1beta/{namePath}?key={apiKey}";

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(url, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Lỗi kết nối HTTP khi kiểm tra trạng thái video. Chi tiết: {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Veo Operations API Error: Status={Status}, Body={Body}", response.StatusCode, errBody);
            throw new BusinessException($"Lỗi từ phía AI Video Provider ({response.StatusCode}): {errBody}");
        }

        var jsonElement = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

        var done = false;
        if (jsonElement.TryGetProperty("done", out var doneProperty))
        {
            done = doneProperty.GetBoolean();
        }

        string? videoUrl = null;
        string? errorMessage = null;

        if (jsonElement.TryGetProperty("error", out var errorProperty))
        {
            errorProperty.TryGetProperty("message", out var msgProp);
            errorMessage = msgProp.GetString() ?? "Lỗi không xác định từ Veo.";
            _logger.LogError("Veo video generation failed on server: {Error}", errorMessage);
        }

        if (done && errorMessage == null && jsonElement.TryGetProperty("response", out var responseProperty))
        {
            videoUrl = ExtractVideoUrl(responseProperty);
        }

        int? progressPercent = done ? 100 : 50; // simple fallback if progress not explicitly returned

        return new VideoOperationStatusResult
        {
            OperationId = operationId,
            Done = done || errorMessage != null,
            VideoUrl = videoUrl,
            ProgressPercent = progressPercent,
            ErrorMessage = errorMessage
        };
    }

    private static string? ExtractVideoUrl(JsonElement responseProperty)
    {
        if (responseProperty.TryGetProperty("generatedVideos", out var generatedVideos) && 
            generatedVideos.GetArrayLength() > 0)
        {
            var firstVideo = generatedVideos[0];
            if (firstVideo.TryGetProperty("video", out var videoObj) && 
                videoObj.TryGetProperty("uri", out var uriProp))
            {
                return uriProp.GetString();
            }
        }
        return null;
    }
}
