using System.Threading;
using System.Threading.Tasks;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Models.Publishing;

namespace RealSync.Core.Interfaces.Publishing;

public interface IPublishingConnector
{
    PublishingChannelType ChannelType { get; }

    Task<ChannelCapabilities> GetCapabilitiesAsync(
        ConnectedAccount? account,
        CancellationToken cancellationToken);

    Task<ValidationResult> ValidateAsync(
        PublicationContext context,
        CancellationToken cancellationToken);

    Task<PublishInitiationResult> PublishAsync(
        PublicationContext context,
        CancellationToken cancellationToken);

    Task<RemotePublicationStatus> GetStatusAsync(
        PublicationTrackingContext context,
        CancellationToken cancellationToken);

    Task<DeleteRemoteResult> DeleteAsync(
        PublicationTrackingContext context,
        CancellationToken cancellationToken);
}
