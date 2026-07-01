using RealSync.Core.Enums;
using RealSync.Shared.DTOs.Responses.Publishing;

namespace RealSync.Core.Interfaces;

public interface IPublicationService
{
    Task<(IEnumerable<PublicationJobResponse> Items, int TotalCount)> GetJobsAsync(
        Guid? postId, PublicationJobStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PublicationJobResponse> GetJobByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<PublicationAttemptResponse>> GetAttemptsAsync(Guid jobId, CancellationToken cancellationToken = default);
}
