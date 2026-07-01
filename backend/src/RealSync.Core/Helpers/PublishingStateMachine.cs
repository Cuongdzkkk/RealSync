using System.Collections.Generic;
using RealSync.Core.Enums;

namespace RealSync.Core.Helpers;

public static class PublishingStateMachine
{
    private static readonly Dictionary<PostWorkflowStatus, HashSet<PostWorkflowStatus>> ValidPostTransitions = new()
    {
        { PostWorkflowStatus.Draft, new() { PostWorkflowStatus.InReview, PostWorkflowStatus.Archived } },
        { PostWorkflowStatus.InReview, new() { PostWorkflowStatus.Approved, PostWorkflowStatus.Rejected, PostWorkflowStatus.Draft } },
        { PostWorkflowStatus.Rejected, new() { PostWorkflowStatus.Draft, PostWorkflowStatus.Archived } },
        { PostWorkflowStatus.Approved, new() { PostWorkflowStatus.Scheduled, PostWorkflowStatus.Publishing, PostWorkflowStatus.InReview, PostWorkflowStatus.Archived } },
        { PostWorkflowStatus.Scheduled, new() { PostWorkflowStatus.Publishing, PostWorkflowStatus.Cancelled, PostWorkflowStatus.Archived } },
        { PostWorkflowStatus.Publishing, new() { PostWorkflowStatus.Published, PostWorkflowStatus.PartiallyPublished, PostWorkflowStatus.Failed } },
        { PostWorkflowStatus.PartiallyPublished, new() { PostWorkflowStatus.Published, PostWorkflowStatus.Failed, PostWorkflowStatus.Draft } },
        { PostWorkflowStatus.Published, new() { PostWorkflowStatus.Archived } },
        { PostWorkflowStatus.Failed, new() { PostWorkflowStatus.Draft, PostWorkflowStatus.Publishing, PostWorkflowStatus.Archived } },
        { PostWorkflowStatus.Cancelled, new() { PostWorkflowStatus.Draft, PostWorkflowStatus.Scheduled, PostWorkflowStatus.Archived } },
        { PostWorkflowStatus.Archived, new() } // terminal
    };

    private static readonly Dictionary<PublicationJobStatus, HashSet<PublicationJobStatus>> ValidJobTransitions = new()
    {
        { PublicationJobStatus.Pending, new() { PublicationJobStatus.Validating, PublicationJobStatus.Cancelled } },
        { PublicationJobStatus.Validating, new() { PublicationJobStatus.Queued, PublicationJobStatus.NeedsReview, PublicationJobStatus.Cancelled } },
        { PublicationJobStatus.Queued, new() { PublicationJobStatus.Publishing, PublicationJobStatus.Cancelled } },
        { PublicationJobStatus.Publishing, new() { PublicationJobStatus.RemoteProcessing, PublicationJobStatus.Published, PublicationJobStatus.RetryScheduled, PublicationJobStatus.NeedsReview, PublicationJobStatus.Failed, PublicationJobStatus.Cancelled } },
        { PublicationJobStatus.RemoteProcessing, new() { PublicationJobStatus.Published, PublicationJobStatus.Failed, PublicationJobStatus.Cancelled } },
        { PublicationJobStatus.RetryScheduled, new() { PublicationJobStatus.Queued, PublicationJobStatus.Cancelled } },
        { PublicationJobStatus.NeedsReview, new() { PublicationJobStatus.Queued, PublicationJobStatus.Failed, PublicationJobStatus.Cancelled } },
        { PublicationJobStatus.Published, new() }, // terminal
        { PublicationJobStatus.Failed, new() { PublicationJobStatus.Queued, PublicationJobStatus.Cancelled } },
        { PublicationJobStatus.Cancelled, new() } // terminal
    };

    public static bool CanTransition(PostWorkflowStatus current, PostWorkflowStatus target)
    {
        return current == target || (ValidPostTransitions.TryGetValue(current, out var allowed) && allowed.Contains(target));
    }

    public static bool CanTransition(PublicationJobStatus current, PublicationJobStatus target)
    {
        return current == target || (ValidJobTransitions.TryGetValue(current, out var allowed) && allowed.Contains(target));
    }
}
