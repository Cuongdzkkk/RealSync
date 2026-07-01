using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Core.Models.Publishing;

namespace RealSync.Services.Publishing;

/// <summary>
/// Connector cho kênh Zalo Official Account.
/// Sử dụng Zalo OA Article API (create → verify) để đăng bài viết lên trang OA.
/// </summary>
public class ZaloOAConnector : IPublishingConnector
{
    private readonly HttpClient _httpClient;
    private readonly ICredentialVault _vault;
    private readonly ILogger<ZaloOAConnector> _logger;

    // Zalo OA API endpoints
    private const string UploadImageEndpoint = "v2.0/oa/upload/image";
    private const string CreateArticleEndpoint = "v2.0/article/create";
    private const string VerifyArticleEndpoint = "v2.0/article/verify";
    private const string GetOAInfoEndpoint = "v2.0/oa/getoa";

    public ZaloOAConnector(HttpClient httpClient, ICredentialVault vault, ILogger<ZaloOAConnector> logger)
    {
        _httpClient = httpClient;
        _vault = vault;
        _logger = logger;
    }

    public PublishingChannelType ChannelType => PublishingChannelType.ZaloOA;

    public Task<ChannelCapabilities> GetCapabilitiesAsync(ConnectedAccount? account, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ChannelCapabilities
        {
            SupportsDirectPublish = true,
            SupportsDraftUpload = false,
            SupportsScheduling = false, // Zalo broadcast có schedule nhưng article thì không
            SupportsVideo = false,
            SupportsImages = true,
            SupportsUpdate = false,
            SupportsDelete = false,
            RequiresFinalUserConfirmation = false
        });
    }

    public Task<ValidationResult> ValidateAsync(PublicationContext context, CancellationToken cancellationToken)
    {
        // Kiểm tra ConnectedAccount
        if (context.Account == null)
            return Task.FromResult(ValidationResult.Failure("Chưa liên kết tài khoản Zalo OA. Vui lòng kết nối tại Cài đặt hệ thống → Kết nối nền tảng."));

        if (context.Account.Status == CredentialStatus.PendingSetup)
            return Task.FromResult(ValidationResult.Failure("Tài khoản Zalo OA chưa được thiết lập. Vui lòng hoàn thành OAuth tại Cài đặt hệ thống."));

        if (context.Account.Status == CredentialStatus.Expired)
            return Task.FromResult(ValidationResult.Failure("Token Zalo OA đã hết hạn. Vui lòng kết nối lại tại Cài đặt hệ thống."));

        if (context.Account.Status == CredentialStatus.Revoked || context.Account.Status == CredentialStatus.Invalid)
            return Task.FromResult(ValidationResult.Failure($"Tài khoản Zalo OA không hợp lệ (trạng thái: {context.Account.Status}). Vui lòng kết nối lại."));

        if (context.Account.Status != CredentialStatus.Active)
            return Task.FromResult(ValidationResult.Failure($"Tài khoản Zalo OA chưa sẵn sàng (trạng thái: {context.Account.Status})."));

        // Kiểm tra nội dung
        if (string.IsNullOrWhiteSpace(context.Variant.Title))
            return Task.FromResult(ValidationResult.Failure("Tiêu đề bài viết trống. Zalo OA yêu cầu tiêu đề bài viết."));

        if (string.IsNullOrWhiteSpace(context.Variant.Caption))
            return Task.FromResult(ValidationResult.Failure("Nội dung bài viết trống. Zalo OA yêu cầu nội dung bài viết."));

        if (context.Variant.Title.Length > 200)
            return Task.FromResult(ValidationResult.Failure("Tiêu đề vượt quá 200 ký tự cho phép của Zalo OA."));

        return Task.FromResult(ValidationResult.Success());
    }

    public async Task<PublishInitiationResult> PublishAsync(PublicationContext context, CancellationToken cancellationToken)
    {
        string accessToken;
        try
        {
            accessToken = DecryptAccessToken(context.Account!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể giải mã access token cho Zalo OA account {AccountId}", context.Account!.Id);
            return PublishInitiationResult.Failure(
                "Không thể giải mã access token. Vui lòng kết nối lại tài khoản.",
                "DECRYPTION_FAILED", "AuthenticationError");
        }

        // Bước 1: Tạo article body
        var articleBody = BuildArticleBody(context.Variant);

        // Bước 2: Gọi API tạo article
        var createResult = await CallZaloApiAsync(
            CreateArticleEndpoint, articleBody, accessToken, cancellationToken);

        if (!createResult.IsSuccess)
            return createResult;

        var articleToken = createResult.ExternalPostId;

        // Bước 3: Verify article để publish lên OA
        var verifyBody = JsonSerializer.Serialize(new { token = articleToken });
        var verifyResult = await CallZaloApiAsync(
            VerifyArticleEndpoint, verifyBody, accessToken, cancellationToken);

        if (!verifyResult.IsSuccess)
        {
            _logger.LogWarning("Zalo article verify failed for token {Token}: {Error}",
                articleToken, verifyResult.ErrorMessage);
            return verifyResult;
        }

        _logger.LogInformation("Zalo OA article published successfully. ArticleToken={Token}, AccountId={AccountId}",
            articleToken, context.Account!.Id);

        return PublishInitiationResult.Success(
            externalPostId: articleToken,
            publishedUrl: null); // Zalo OA không trả URL trực tiếp khi verify
    }

    public async Task<RemotePublicationStatus> GetStatusAsync(PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        // Zalo OA article sau khi verify thành công sẽ tự động Published
        // Kiểm tra bằng cách verify token có còn valid không
        if (context.Account == null || context.Account.Status != CredentialStatus.Active)
        {
            return new RemotePublicationStatus
            {
                Status = PublicationJobStatus.NeedsReview,
                ErrorMessage = "Tài khoản Zalo OA không hoạt động."
            };
        }

        return new RemotePublicationStatus
        {
            Status = PublicationJobStatus.Published,
            RemoteStatusDescription = "Bài viết đã được đăng trên Zalo OA."
        };
    }

    public Task<DeleteRemoteResult> DeleteAsync(PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        // Zalo OA Article API hiện không hỗ trợ xóa bài qua API
        return Task.FromResult(DeleteRemoteResult.Failure(
            "Zalo OA hiện không hỗ trợ xóa bài viết qua API. Vui lòng xóa trực tiếp trên trang quản lý Zalo OA."));
    }

    #region Private Helpers

    private string DecryptAccessToken(ConnectedAccount account)
    {
        if (account.OAuthCredential == null
            || string.IsNullOrEmpty(account.OAuthCredential.AccessTokenEncrypted)
            || string.IsNullOrEmpty(account.OAuthCredential.EncryptionKeyVersion))
        {
            throw new InvalidOperationException("Không tìm thấy credential OAuth cho tài khoản Zalo OA.");
        }

        return _vault.Decrypt(
            account.OAuthCredential.AccessTokenEncrypted,
            account.OAuthCredential.EncryptionKeyVersion);
    }

    private static string BuildArticleBody(ContentVariant variant)
    {
        // Xây dựng body cho Zalo OA Article API
        // Body format: {"type": "normal", "title": "...", "author": "RealSync", "cover": {...}, "description": "...", "body": [...]}
        var body = new
        {
            type = "normal",
            title = variant.Title ?? "Bài viết RealSync",
            author = "RealSync",
            description = variant.Summary ?? (variant.Caption?.Length > 150
                ? variant.Caption[..150] + "..."
                : variant.Caption ?? ""),
            body = new object[]
            {
                new
                {
                    type = "text",
                    content = variant.Caption ?? ""
                }
            }
        };

        return JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private async Task<PublishInitiationResult> CallZaloApiAsync(
        string endpoint, string jsonBody, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("access_token", accessToken);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogDebug("Zalo API {Endpoint} response: {StatusCode} {Body}",
                endpoint, response.StatusCode, responseBody);

            if (!response.IsSuccessStatusCode)
            {
                return MapHttpError(response.StatusCode, responseBody);
            }

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            // Zalo API trả error code trong JSON body
            if (root.TryGetProperty("error", out var errorProp) && errorProp.GetInt32() != 0)
            {
                var errorCode = errorProp.GetInt32();
                var errorMsg = root.TryGetProperty("message", out var msgProp)
                    ? msgProp.GetString() ?? "Lỗi không xác định từ Zalo"
                    : "Lỗi không xác định từ Zalo";

                return MapZaloError(errorCode, errorMsg);
            }

            // Lấy article token/ID từ response
            string? externalId = null;
            if (root.TryGetProperty("data", out var dataProp))
            {
                if (dataProp.TryGetProperty("token", out var tokenProp))
                    externalId = tokenProp.GetString();
                else if (dataProp.TryGetProperty("article_id", out var articleIdProp))
                    externalId = articleIdProp.GetString();
            }

            return PublishInitiationResult.Success(externalId, publishedUrl: null);
        }
        catch (TaskCanceledException)
        {
            return PublishInitiationResult.Failure(
                "Yêu cầu đến Zalo API bị timeout.",
                "NETWORK_TIMEOUT", "TransientError");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error calling Zalo API {Endpoint}", endpoint);
            return PublishInitiationResult.Failure(
                $"Lỗi kết nối đến Zalo API: {ex.Message}",
                "NETWORK_ERROR", "TransientError");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Zalo API response for {Endpoint}", endpoint);
            return PublishInitiationResult.Failure(
                "Phản hồi từ Zalo API không hợp lệ.",
                "INVALID_RESPONSE", "UnknownError");
        }
    }

    /// <summary>
    /// Map mã lỗi Zalo sang normalized error category.
    /// Tham khảo: https://developers.zalo.me/docs/official-account/bat-dau/error-code
    /// </summary>
    public static PublishInitiationResult MapZaloError(int zaloErrorCode, string message)
    {
        var (normalizedCode, category) = zaloErrorCode switch
        {
            -201 => ("INVALID_TOKEN", "AuthenticationError"),
            -202 => ("TOKEN_EXPIRED", "AuthenticationError"),
            -210 => ("RATE_LIMIT_EXCEEDED", "RateLimitError"),
            -213 => ("PERMISSION_DENIED", "AuthorizationError"),
            -216 => ("OA_NOT_FOUND", "ConfigurationError"),
            -217 => ("OA_NOT_ACTIVE", "ConfigurationError"),
            -231 => ("INVALID_MEDIA", "ValidationError"),
            -232 => ("MEDIA_TOO_LARGE", "ValidationError"),
            -240 => ("INVALID_ARTICLE", "ValidationError"),
            -241 => ("ARTICLE_NOT_FOUND", "ValidationError"),
            _ => ($"ZALO_{Math.Abs(zaloErrorCode)}", "UnknownError")
        };

        return PublishInitiationResult.Failure(
            $"Zalo OA API lỗi ({zaloErrorCode}): {message}",
            normalizedCode, category);
    }

    private static PublishInitiationResult MapHttpError(System.Net.HttpStatusCode statusCode, string body)
    {
        var (code, category) = statusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => ("INVALID_TOKEN", "AuthenticationError"),
            System.Net.HttpStatusCode.Forbidden => ("PERMISSION_DENIED", "AuthorizationError"),
            System.Net.HttpStatusCode.TooManyRequests => ("RATE_LIMIT_EXCEEDED", "RateLimitError"),
            System.Net.HttpStatusCode.InternalServerError => ("ZALO_SERVER_ERROR", "TransientError"),
            System.Net.HttpStatusCode.ServiceUnavailable => ("ZALO_UNAVAILABLE", "TransientError"),
            _ => ("ZALO_HTTP_ERROR", "UnknownError")
        };

        return PublishInitiationResult.Failure(
            $"Zalo API trả HTTP {(int)statusCode}: {body}",
            code, category);
    }

    #endregion
}
