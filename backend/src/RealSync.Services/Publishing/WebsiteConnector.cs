using System;
using System.Threading;
using System.Threading.Tasks;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Core.Models.Publishing;

namespace RealSync.Services.Publishing;

public class WebsiteConnector : IPublishingConnector
{
    public PublishingChannelType ChannelType => PublishingChannelType.Website;

    public Task<ChannelCapabilities> GetCapabilitiesAsync(ConnectedAccount? account, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ChannelCapabilities());
    }

    public Task<ValidationResult> ValidateAsync(PublicationContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(context.Variant.Title))
            return Task.FromResult(ValidationResult.Failure("Tiêu đề nội dung trống."));
        if (string.IsNullOrWhiteSpace(context.Variant.Caption))
            return Task.FromResult(ValidationResult.Failure("Nội dung bài viết trống."));
        return Task.FromResult(ValidationResult.Success());
    }

    public Task<PublishInitiationResult> PublishAsync(PublicationContext context, CancellationToken cancellationToken)
    {
        var publishedUrl = $"/api/v1/posts/{context.Job.PostId}/public";
        return Task.FromResult(PublishInitiationResult.Success(context.Job.PostId.ToString(), publishedUrl));
    }

    public Task<RemotePublicationStatus> GetStatusAsync(PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(new RemotePublicationStatus
        {
            Status = PublicationJobStatus.Published,
            PublishedUrl = $"/api/v1/posts/{context.Job.PostId}/public"
        });
    }

    public Task<DeleteRemoteResult> DeleteAsync(PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(DeleteRemoteResult.Success());
    }
}
