using FluentAssertions;
using NUnit.Framework;
using RealSync.Core.Enums;
using RealSync.Core.Helpers;

namespace RealSync.UnitTests.Publishing;

[TestFixture]
public class PublishingStateMachineTests
{
    [TestCase(PostWorkflowStatus.Draft, PostWorkflowStatus.InReview, true)]
    [TestCase(PostWorkflowStatus.Draft, PostWorkflowStatus.Archived, true)]
    [TestCase(PostWorkflowStatus.Draft, PostWorkflowStatus.Approved, false)]
    [TestCase(PostWorkflowStatus.InReview, PostWorkflowStatus.Approved, true)]
    [TestCase(PostWorkflowStatus.InReview, PostWorkflowStatus.Rejected, true)]
    [TestCase(PostWorkflowStatus.InReview, PostWorkflowStatus.Draft, true)]
    [TestCase(PostWorkflowStatus.InReview, PostWorkflowStatus.Scheduled, false)]
    [TestCase(PostWorkflowStatus.Approved, PostWorkflowStatus.Scheduled, true)]
    [TestCase(PostWorkflowStatus.Approved, PostWorkflowStatus.Publishing, true)]
    [TestCase(PostWorkflowStatus.Approved, PostWorkflowStatus.Draft, false)]
    [TestCase(PostWorkflowStatus.Scheduled, PostWorkflowStatus.Publishing, true)]
    [TestCase(PostWorkflowStatus.Scheduled, PostWorkflowStatus.Cancelled, true)]
    [TestCase(PostWorkflowStatus.Scheduled, PostWorkflowStatus.Draft, false)]
    [TestCase(PostWorkflowStatus.Publishing, PostWorkflowStatus.Published, true)]
    [TestCase(PostWorkflowStatus.Publishing, PostWorkflowStatus.PartiallyPublished, true)]
    [TestCase(PostWorkflowStatus.Publishing, PostWorkflowStatus.Failed, true)]
    [TestCase(PostWorkflowStatus.Publishing, PostWorkflowStatus.Draft, false)]
    [TestCase(PostWorkflowStatus.Published, PostWorkflowStatus.Archived, true)]
    [TestCase(PostWorkflowStatus.Published, PostWorkflowStatus.Draft, false)]
    public void CanTransition_PostWorkflowStatus_ShouldBeValidatedCorrectly(PostWorkflowStatus current, PostWorkflowStatus target, bool expectedResult)
    {
        bool result = PublishingStateMachine.CanTransition(current, target);
        result.Should().Be(expectedResult);
    }

    [TestCase(PublicationJobStatus.Pending, PublicationJobStatus.Validating, true)]
    [TestCase(PublicationJobStatus.Pending, PublicationJobStatus.Cancelled, true)]
    [TestCase(PublicationJobStatus.Pending, PublicationJobStatus.Published, false)]
    [TestCase(PublicationJobStatus.Validating, PublicationJobStatus.Queued, true)]
    [TestCase(PublicationJobStatus.Validating, PublicationJobStatus.NeedsReview, true)]
    [TestCase(PublicationJobStatus.Validating, PublicationJobStatus.Cancelled, true)]
    [TestCase(PublicationJobStatus.Queued, PublicationJobStatus.Publishing, true)]
    [TestCase(PublicationJobStatus.Queued, PublicationJobStatus.Cancelled, true)]
    [TestCase(PublicationJobStatus.Publishing, PublicationJobStatus.Published, true)]
    [TestCase(PublicationJobStatus.Publishing, PublicationJobStatus.Failed, true)]
    [TestCase(PublicationJobStatus.Publishing, PublicationJobStatus.RetryScheduled, true)]
    [TestCase(PublicationJobStatus.Publishing, PublicationJobStatus.RemoteProcessing, true)]
    [TestCase(PublicationJobStatus.RemoteProcessing, PublicationJobStatus.Published, true)]
    [TestCase(PublicationJobStatus.RemoteProcessing, PublicationJobStatus.Failed, true)]
    [TestCase(PublicationJobStatus.RetryScheduled, PublicationJobStatus.Queued, true)]
    [TestCase(PublicationJobStatus.Failed, PublicationJobStatus.Queued, true)]
    [TestCase(PublicationJobStatus.Published, PublicationJobStatus.Queued, false)]
    public void CanTransition_PublicationJobStatus_ShouldBeValidatedCorrectly(PublicationJobStatus current, PublicationJobStatus target, bool expectedResult)
    {
        bool result = PublishingStateMachine.CanTransition(current, target);
        result.Should().Be(expectedResult);
    }
}
