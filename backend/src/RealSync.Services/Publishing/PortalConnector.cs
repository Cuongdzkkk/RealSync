using System;
using System.Threading;
using System.Threading.Tasks;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Core.Models.Publishing;

namespace RealSync.Services.Publishing;

/// <summary>
/// Connector nen tang portal BDS.
/// Phase 8 chi bat AssistedPosting/feed foundation khi chua co partner API official.
/// </summary>
public class PortalConnector : IPublishingConnector
{
    public PublishingChannelType ChannelType => PublishingChannelType.Portal;

    public Task<ChannelCapabilities> GetCapabilitiesAsync(ConnectedAccount? account, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ChannelCapabilities
        {
            SupportsDirectPublish = false,
            SupportsDraftUpload = false,
            SupportsScheduling = true,
            SupportsVideo = false,
            SupportsImages = true,
            SupportsUpdate = false,
            SupportsDelete = false,
            RequiresFinalUserConfirmation = true,
            GrantedScopes = PublishingJson.ParseScopes(account?.GrantedScopesJson),
            RestrictionReason = "Portal chua co partner API/credential official. Su dung Assisted Posting hoac feed export."
        });
    }

    public Task<ValidationResult> ValidateAsync(PublicationContext context, CancellationToken cancellationToken)
    {
        if (context.Job.PublishMode != PublishMode.Assisted)
        {
            return Task.FromResult(ValidationResult.Failure(
                "Portal chi ho tro Assisted trong Phase 8 neu chua co partner API official."));
        }

        if (string.IsNullOrWhiteSpace(context.Variant.Title))
            return Task.FromResult(ValidationResult.Failure("Portal assisted package yeu cau tieu de."));

        if (string.IsNullOrWhiteSpace(context.Variant.Caption))
            return Task.FromResult(ValidationResult.Failure("Portal assisted package yeu cau noi dung tin dang."));

        return Task.FromResult(ValidationResult.Success());
    }

    public Task<PublishInitiationResult> PublishAsync(PublicationContext context, CancellationToken cancellationToken)
    {
        context.Job.RemoteStatus = "ASSISTED_POSTING_PACKAGE_READY";
        return Task.FromResult(PublishInitiationResult.RemoteProcessing(
            $"portal-assisted-{context.Job.Id:N}",
            context.Job.RemoteStatus));
    }

    public Task<RemotePublicationStatus> GetStatusAsync(PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(new RemotePublicationStatus
        {
            Status = PublicationJobStatus.NeedsReview,
            RemoteStatusDescription =
                "Portal package da san sang. Nguoi dung can dang thu cong hoac cau hinh partner API/feed export de hoan tat.",
            ErrorMessage = "Chua co external URL/partner confirmation nen khong the danh dau Published."
        });
    }

    public Task<DeleteRemoteResult> DeleteAsync(PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(DeleteRemoteResult.Failure(
            "Portal assisted posting khong co remote object de xoa qua API trong Phase 8."));
    }
}
