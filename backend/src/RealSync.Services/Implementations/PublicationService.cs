using Microsoft.EntityFrameworkCore;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Responses.Publishing;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Implementations;

public class PublicationService : IPublicationService
{
    private readonly RealSyncDbContext _context;

    public PublicationService(RealSyncDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<PublicationJobResponse> Items, int TotalCount)> GetJobsAsync(
        Guid? postId, PublicationJobStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _context.PublicationJobs.AsQueryable();

        if (postId.HasValue)
        {
            query = query.Where(j => j.PostId == postId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(j => j.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new PublicationJobResponse(
                j.Id, j.PostId, j.ContentVariantId, j.ConnectedAccountId, j.PublishMode,
                j.ScheduledAtUtc, j.Status, j.IdempotencyKey, j.ExternalPostId, j.PublishedUrl,
                j.PublishedAt, j.LastErrorMessage, j.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<PublicationJobResponse> GetJobByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _context.PublicationJobs.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new NotFoundException("PublicationJob", id);

        return new PublicationJobResponse(
            job.Id, job.PostId, job.ContentVariantId, job.ConnectedAccountId, job.PublishMode,
            job.ScheduledAtUtc, job.Status, job.IdempotencyKey, job.ExternalPostId, job.PublishedUrl,
            job.PublishedAt, job.LastErrorMessage, job.CreatedAt);
    }

    public async Task<IEnumerable<PublicationAttemptResponse>> GetAttemptsAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return await _context.PublicationAttempts
            .Where(a => a.PublicationJobId == jobId)
            .OrderByDescending(a => a.StartedAt)
            .Select(a => new PublicationAttemptResponse(
                a.Id, a.PublicationJobId, a.AttemptNumber, a.StartedAt, a.CompletedAt, a.DurationMs,
                a.ProviderHttpStatus, a.ProviderErrorCode, a.NormalizedErrorCategory, a.ProviderRequestId,
                a.RequestMetadataJson, a.ResponseMetadataJson, a.IsSuccess, a.RetryDecision, a.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}
