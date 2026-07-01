using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Core.Models.Publishing;

namespace RealSync.Services.Publishing;

/// <summary>
/// Connector tự động đăng bài cho Facebook Group sử dụng trình duyệt giả lập.
/// </summary>
public class FacebookGroupConnector : IPublishingConnector
{
    public PublishingChannelType ChannelType => PublishingChannelType.FacebookGroup;

    public Task<ChannelCapabilities> GetCapabilitiesAsync(ConnectedAccount? account, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ChannelCapabilities
        {
            SupportsDirectPublish = true,
            SupportsDraftUpload = false,
            SupportsScheduling = true,
            SupportsVideo = true,
            SupportsImages = true,
            SupportsUpdate = false,
            SupportsDelete = false,
            RequiresFinalUserConfirmation = false,
            GrantedScopes = PublishingJson.ParseScopes(account?.GrantedScopesJson),
            RestrictionReason = null
        });
    }

    public Task<ValidationResult> ValidateAsync(PublicationContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(context.Variant.Caption))
            return Task.FromResult(ValidationResult.Failure("Bài đăng Facebook Group yêu cầu phần mô tả/caption."));

        return Task.FromResult(ValidationResult.Success());
    }

    public async Task<PublishInitiationResult> PublishAsync(PublicationContext context, CancellationToken cancellationToken)
    {
        var groupUrl = context.Account?.ProfileUrl;
        if (string.IsNullOrWhiteSpace(groupUrl))
        {
            return PublishInitiationResult.Failure("Thiếu URL Group Facebook của tài khoản liên kết. Vui lòng cập nhật Profile URL.");
        }

        var jobIdStr = Guid.NewGuid().ToString();
        var crawlerPath = Path.Combine("d:\\A\\RealSync\\crawler", "src", "facebook_stealth.js");
        var resultsDir = Path.Combine("d:\\A\\RealSync\\crawler", "results");
        var resultFilePath = Path.Combine(resultsDir, $"{jobIdStr}.json");

        string mediaArg = "";
        if (!string.IsNullOrWhiteSpace(context.Job.MediaManifestJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(context.Job.MediaManifestJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("videoUrl", out var videoEl))
                {
                    mediaArg = videoEl.GetString() ?? "";
                }
                else if (root.TryGetProperty("images", out var imagesEl) && imagesEl.ValueKind == JsonValueKind.Array && imagesEl.GetArrayLength() > 0)
                {
                    mediaArg = imagesEl[0].GetString() ?? "";
                }
            }
            catch { /* fallback */ }
        }

        // Đảm bảo thư mục results tồn tại
        if (!Directory.Exists(resultsDir))
        {
            Directory.CreateDirectory(resultsDir);
        }

        var cleanCaption = context.Variant.Caption?.Replace("\"", "\\\"").Replace("\r\n", " ").Replace("\n", " ") ?? "";

        var startInfo = new ProcessStartInfo
        {
            FileName = "node",
            Arguments = $"\"{crawlerPath}\" --url \"{groupUrl}\" --jobId \"{jobIdStr}\" --mode \"fb-post\" --content \"{cleanCaption}\" --media \"{mediaArg.Replace("\"", "\\\"")}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = "d:\\A\\RealSync\\crawler"
        };

        try
        {
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            await process.WaitForExitAsync(cancellationToken);

            if (File.Exists(resultFilePath))
            {
                var jsonContent = await File.ReadAllTextAsync(resultFilePath, cancellationToken);
                using var doc = JsonDocument.Parse(jsonContent);
                var root = doc.RootElement;
                if (root.TryGetProperty("success", out var successEl) && successEl.GetBoolean())
                {
                    try { File.Delete(resultFilePath); } catch {}
                    context.Job.RemoteStatus = "PUBLISHED";
                    return PublishInitiationResult.Success($"fb-{jobIdStr}", groupUrl);
                }
                else
                {
                    string err = root.TryGetProperty("error", out var errEl) ? errEl.GetString() ?? "Lỗi không xác định" : "Lỗi đăng bài";
                    try { File.Delete(resultFilePath); } catch {}
                    return PublishInitiationResult.Failure(err);
                }
            }
            return PublishInitiationResult.Failure("Tiến trình tự động đăng bài kết thúc nhưng không trả về tệp kết quả.");
        }
        catch (Exception ex)
        {
            return PublishInitiationResult.Failure($"Không thể chạy trình tự động đăng bài Facebook. Chi tiết: {ex.Message}");
        }
    }

    public Task<RemotePublicationStatus> GetStatusAsync(PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        if (context.Job.RemoteStatus == "PUBLISHED")
        {
            return Task.FromResult(new RemotePublicationStatus
            {
                Status = PublicationJobStatus.Published,
                RemoteStatusDescription = "Bài đăng đã được xuất bản lên Facebook Group bằng trình duyệt tự động hóa.",
                PublishedUrl = context.Job.PublishedUrl
            });
        }

        return Task.FromResult(new RemotePublicationStatus
        {
            Status = PublicationJobStatus.Failed,
            RemoteStatusDescription = "Đăng bài thất bại hoặc chưa hoàn tất."
        });
    }

    public Task<DeleteRemoteResult> DeleteAsync(PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(DeleteRemoteResult.Failure("Không hỗ trợ xóa bài đăng tự động trên Facebook Group."));
    }
}
