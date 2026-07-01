using System;
using System.Threading;
using System.Threading.Tasks;
using RealSync.Shared.DTOs.Requests.Publishing;
using RealSync.Shared.DTOs.Responses.Publishing;

namespace RealSync.Core.Interfaces.Publishing;

public interface IPublicationOrchestrator
{
    Task<PublicationJobResponse> QueueAsync(
        QueuePublicationRequest request,
        CancellationToken cancellationToken);

    Task ExecuteAsync(
        Guid publicationJobId,
        CancellationToken cancellationToken);

    Task<PublicationJobResponse> RetryAsync(
        Guid publicationJobId,
        CancellationToken cancellationToken);

    Task CancelAsync(
        Guid publicationJobId,
        CancellationToken cancellationToken);

    Task RefreshRemoteStatusAsync(
        Guid publicationJobId,
        CancellationToken cancellationToken);
}
