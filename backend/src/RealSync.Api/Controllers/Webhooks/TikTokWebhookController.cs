using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealSync.Core.Enums;
using RealSync.Data.Context;

namespace RealSync.Api.Controllers.Webhooks;

/// <summary>
/// Webhook endpoint cho TikTok Content Posting events.
/// </summary>
[ApiController]
[Route("api/v1/webhooks/tiktok")]
[AllowAnonymous]
public class TikTokWebhookController : ControllerBase
{
    private readonly RealSyncDbContext _context;
    private readonly ILogger<TikTokWebhookController> _logger;

    public TikTokWebhookController(RealSyncDbContext context, ILogger<TikTokWebhookController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);

        _logger.LogInformation("TikTok webhook received. Length={Length}", body.Length);

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var eventName = root.TryGetProperty("event", out var ev) ? ev.GetString() : null;
            if (string.IsNullOrEmpty(eventName) && root.TryGetProperty("content", out var content))
            {
                eventName = content.TryGetProperty("event", out var cev) ? cev.GetString() : null;
            }

            string? publishId = null;
            if (root.TryGetProperty("publish_id", out var pid))
                publishId = pid.GetString();
            else if (root.TryGetProperty("content", out var contentProp)
                && contentProp.TryGetProperty("publish_id", out var cpid))
                publishId = cpid.GetString();

            if (!string.IsNullOrEmpty(publishId))
            {
                var job = await _context.PublicationJobs
                    .FirstOrDefaultAsync(j => j.ExternalPublishId == publishId, cancellationToken);

                if (job != null)
                {
                    job.RemoteStatus = eventName ?? "webhook_received";

                    if (eventName is "post.publish.complete" or "post.publish.publicly_available")
                    {
                        job.Status = PublicationJobStatus.Published;
                        job.PublishedAt = DateTime.UtcNow;
                    }
                    else if (eventName == "post.publish.failed")
                    {
                        job.Status = PublicationJobStatus.Failed;
                        job.LastErrorMessage = "TikTok webhook báo publish thất bại.";
                    }
                    else if (eventName == "post.publish.inbox_delivered")
                    {
                        job.Status = PublicationJobStatus.RemoteProcessing;
                        job.RemoteStatus = "SEND_TO_USER_INBOX";
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "TikTok webhook payload không hợp lệ");
        }

        return Ok();
    }
}
