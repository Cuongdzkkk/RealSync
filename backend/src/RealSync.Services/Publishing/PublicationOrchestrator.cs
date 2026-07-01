using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Helpers;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Core.Models.Publishing;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.Publishing;
using RealSync.Shared.DTOs.Responses.Publishing;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Publishing;

public class PublicationOrchestrator : IPublicationOrchestrator
{
    private readonly RealSyncDbContext _context;
    private readonly IConnectorResolver _resolver;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public PublicationOrchestrator(
        RealSyncDbContext context,
        IConnectorResolver resolver,
        IBackgroundJobClient backgroundJobClient)
    {
        _context = context;
        _resolver = resolver;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<PublicationJobResponse> QueueAsync(QueuePublicationRequest request, CancellationToken cancellationToken)
    {
        ContentVariant? variant = null;
        if (request.ContentVariantId != Guid.Empty)
        {
            variant = await _context.ContentVariants.FindAsync(new object[] { request.ContentVariantId }, cancellationToken);
        }

        if (variant == null)
        {
            var post = await _context.Posts.FindAsync(new object[] { request.PostId }, cancellationToken)
                ?? throw new NotFoundException("Post", request.PostId);

            var channelType = PublishingChannelType.Website;
            if (request.ConnectedAccountId.HasValue)
            {
                var account = await _context.ConnectedAccounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == request.ConnectedAccountId.Value, cancellationToken);
                if (account != null)
                    channelType = account.ChannelType;
            }

            variant = new ContentVariant
            {
                Id = Guid.NewGuid(),
                PostId = request.PostId,
                ChannelType = channelType,
                Title = post.Title,
                Caption = post.Content ?? post.Summary ?? "Nội dung đăng tin",
                Status = "Approved",
                Language = "vi",
                Version = 1
            };
            _context.ContentVariants.Add(variant);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var finalVariantId = variant.Id;

        var idempotencyKey = IdempotencyHelper.GenerateKey(
            Guid.Empty, request.PostId, finalVariantId, request.ConnectedAccountId ?? Guid.Empty, request.ScheduledAtUtc, variant.HashtagsJson);

        var existingJob = await _context.PublicationJobs
            .FirstOrDefaultAsync(j => j.IdempotencyKey == idempotencyKey, cancellationToken);
        if (existingJob != null)
        {
            return MapToResponse(existingJob);
        }

        var job = new PublicationJob
        {
            PostId = request.PostId,
            ContentVariantId = finalVariantId,
            ConnectedAccountId = request.ConnectedAccountId,
            PublishMode = request.PublishMode,
            ScheduledAtUtc = request.ScheduledAtUtc,
            Status = PublicationJobStatus.Pending,
            IdempotencyKey = idempotencyKey,
            MaxRetryCount = 5,
            MediaManifestJson = request.MediaManifestJson
        };

        _context.PublicationJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);

        if (request.ScheduledAtUtc == null || request.ScheduledAtUtc <= DateTime.UtcNow)
        {
            _backgroundJobClient.Enqueue<PublicationOrchestrator>(o => o.ExecuteAsync(job.Id, CancellationToken.None));
        }
        else
        {
            var delay = request.ScheduledAtUtc.Value - DateTime.UtcNow;
            _backgroundJobClient.Schedule<PublicationOrchestrator>(o => o.ExecuteAsync(job.Id, CancellationToken.None), delay);
        }

        return MapToResponse(job);
    }

    public async Task ExecuteAsync(Guid publicationJobId, CancellationToken cancellationToken)
    {
        var job = await _context.PublicationJobs
            .Include(j => j.ContentVariant)
            .Include(j => j.ConnectedAccount)
            .FirstOrDefaultAsync(j => j.Id == publicationJobId, cancellationToken);

        if (job == null || job.Status == PublicationJobStatus.Published || job.Status == PublicationJobStatus.Cancelled)
            return;

        job.Status = PublicationJobStatus.Validating;
        await _context.SaveChangesAsync(cancellationToken);

        var connector = _resolver.Resolve(job.ContentVariant.ChannelType);
        var context = new PublicationContext { Job = job, Variant = job.ContentVariant, Account = job.ConnectedAccount };

        var valResult = await connector.ValidateAsync(context, cancellationToken);
        if (!valResult.IsValid)
        {
            job.Status = PublicationJobStatus.NeedsReview;
            job.LastErrorMessage = string.Join("; ", valResult.Errors);
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        job.Status = PublicationJobStatus.Publishing;
        job.LastAttemptAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var attempt = new PublicationAttempt
        {
            PublicationJobId = job.Id,
            AttemptNumber = job.RetryCount + 1,
            StartedAt = DateTime.UtcNow
        };
        _context.PublicationAttempts.Add(attempt);

        try
        {
            var pubResult = await connector.PublishAsync(context, cancellationToken);
            attempt.CompletedAt = DateTime.UtcNow;
            attempt.DurationMs = (long)(attempt.CompletedAt.Value - attempt.StartedAt).TotalMilliseconds;
            attempt.IsSuccess = pubResult.IsSuccess;

            if (pubResult.IsSuccess)
            {
                if (pubResult.RequiresRemoteProcessing)
                {
                    job.Status = PublicationJobStatus.RemoteProcessing;
                    job.ExternalPublishId = pubResult.ExternalPublishId;
                    job.ExternalPostId = pubResult.ExternalPostId;
                    job.RemoteStatus = pubResult.ExternalPostId;
                }
                else
                {
                    job.Status = PublicationJobStatus.Published;
                    job.PublishedUrl = pubResult.PublishedUrl;
                    job.PublishedAt = DateTime.UtcNow;
                    job.ExternalPostId = pubResult.ExternalPostId;
                    job.ExternalPublishId = pubResult.ExternalPublishId;
                }
            }
            else
            {
                attempt.ProviderErrorCode = pubResult.ErrorCode;
                attempt.NormalizedErrorCategory = pubResult.ErrorCategory;

                job.RetryCount++;
                job.LastErrorMessage = pubResult.ErrorMessage;
                job.LastErrorCode = pubResult.ErrorCode;
                job.LastErrorCategory = pubResult.ErrorCategory;

                var shouldRetry = pubResult.IsRetryable && job.RetryCount < job.MaxRetryCount;

                if (shouldRetry)
                {
                    job.Status = PublicationJobStatus.RetryScheduled;
                    var delayMin = Math.Pow(2, job.RetryCount);
                    job.NextRetryAt = DateTime.UtcNow.AddMinutes(delayMin);
                    _backgroundJobClient.Schedule<PublicationOrchestrator>(o => o.ExecuteAsync(job.Id, CancellationToken.None), TimeSpan.FromMinutes(delayMin));
                }
                else if (!pubResult.IsRetryable)
                {
                    job.Status = PublicationJobStatus.NeedsReview;
                }
                else
                {
                    job.Status = PublicationJobStatus.Failed;
                }
            }
        }
        catch (Exception ex)
        {
            attempt.CompletedAt = DateTime.UtcNow;
            attempt.DurationMs = (long)(attempt.CompletedAt.Value - attempt.StartedAt).TotalMilliseconds;
            attempt.IsSuccess = false;
            attempt.ResponseMetadataJson = ex.ToString();

            job.RetryCount++;
            job.LastErrorMessage = ex.Message;

            if (job.RetryCount < job.MaxRetryCount)
            {
                job.Status = PublicationJobStatus.RetryScheduled;
                var delayMin = Math.Pow(2, job.RetryCount);
                job.NextRetryAt = DateTime.UtcNow.AddMinutes(delayMin);
                _backgroundJobClient.Schedule<PublicationOrchestrator>(o => o.ExecuteAsync(job.Id, CancellationToken.None), TimeSpan.FromMinutes(delayMin));
            }
            else
            {
                job.Status = PublicationJobStatus.Failed;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PublicationJobResponse> RetryAsync(Guid publicationJobId, CancellationToken cancellationToken)
    {
        var job = await _context.PublicationJobs.FindAsync(new object[] { publicationJobId }, cancellationToken)
            ?? throw new NotFoundException("PublicationJob", publicationJobId);

        if (job.Status != PublicationJobStatus.Failed && job.Status != PublicationJobStatus.NeedsReview)
            throw new BusinessException("Chỉ có thể retry các job có trạng thái Failed hoặc NeedsReview.");

        job.Status = PublicationJobStatus.Queued;
        job.RetryCount = 0;
        await _context.SaveChangesAsync(cancellationToken);

        _backgroundJobClient.Enqueue<PublicationOrchestrator>(o => o.ExecuteAsync(job.Id, CancellationToken.None));
        return MapToResponse(job);
    }

    public async Task CancelAsync(Guid publicationJobId, CancellationToken cancellationToken)
    {
        var job = await _context.PublicationJobs.FindAsync(new object[] { publicationJobId }, cancellationToken)
            ?? throw new NotFoundException("PublicationJob", publicationJobId);

        if (job.Status == PublicationJobStatus.Published)
            throw new BusinessException("Không thể hủy job đã xuất bản thành công.");

        job.Status = PublicationJobStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RefreshRemoteStatusAsync(Guid publicationJobId, CancellationToken cancellationToken)
    {
        var job = await _context.PublicationJobs
            .Include(j => j.ContentVariant)
            .Include(j => j.ConnectedAccount)
            .FirstOrDefaultAsync(j => j.Id == publicationJobId, cancellationToken)
            ?? throw new NotFoundException("PublicationJob", publicationJobId);

        try
        {
            var connector = _resolver.Resolve(job.ContentVariant.ChannelType);
            var trackingCtx = new PublicationTrackingContext { Job = job, Account = job.ConnectedAccount };

            var status = await connector.GetStatusAsync(trackingCtx, cancellationToken);
            job.Status = status.Status;
            if (status.PublishedUrl != null)
                job.PublishedUrl = status.PublishedUrl;
            if (status.ErrorMessage != null)
                job.LastErrorMessage = status.ErrorMessage;
        }
        catch (Exception ex)
        {
            job.Status = PublicationJobStatus.NeedsReview;
            job.LastErrorMessage = $"Lỗi làm mới trạng thái: {ex.Message}";
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static PublicationJobResponse MapToResponse(PublicationJob job)
    {
        return new PublicationJobResponse(
            job.Id, job.PostId, job.ContentVariantId, job.ConnectedAccountId, job.PublishMode,
            job.ScheduledAtUtc, job.Status, job.IdempotencyKey, job.ExternalPostId, job.PublishedUrl,
            job.PublishedAt, job.LastErrorMessage, job.CreatedAt);
    }
}
