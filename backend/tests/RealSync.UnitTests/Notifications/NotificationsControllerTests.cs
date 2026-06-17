using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealSync.Api.Controllers;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Notifications;
using RealSync.Shared.DTOs.Responses;
using RealSync.Shared.DTOs.Responses.Notifications;

namespace RealSync.UnitTests.Notifications;

[TestFixture]
public class NotificationsControllerTests
{
    [Test]
    public async Task GetMyNotifications_ShouldReturnPagedResponse()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<INotificationService>();
        service.Setup(s => s.GetMyNotificationsAsync(userId, It.IsAny<NotificationQueryDto>()))
            .ReturnsAsync((new List<NotificationDto> { new() { Id = Guid.NewGuid(), Title = "Notification" } }, 1));
        var controller = new NotificationsController(service.Object, NotificationTestFactory.CurrentUser(userId).Object);

        var result = await controller.GetMyNotifications(new NotificationQueryDto { Page = 1, PageSize = 20 });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<IReadOnlyList<NotificationDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Meta!.TotalCount.Should().Be(1);
    }

    [Test]
    public async Task GetMyNotification_ShouldReturnOkResponse()
    {
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var service = new Mock<INotificationService>();
        service.Setup(s => s.GetMyNotificationByIdAsync(userId, notificationId))
            .ReturnsAsync(new NotificationDto { Id = notificationId, Title = "Notification" });
        var controller = new NotificationsController(service.Object, NotificationTestFactory.CurrentUser(userId).Object);

        var result = await controller.GetMyNotification(notificationId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task GetUnreadCount_ShouldReturnOkResponse()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<INotificationService>();
        service.Setup(s => s.GetUnreadCountAsync(userId)).ReturnsAsync(3);
        var controller = new NotificationsController(service.Object, NotificationTestFactory.CurrentUser(userId).Object);

        var result = await controller.GetUnreadCount();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    [Test]
    public async Task GetSummary_ShouldReturnOkResponse()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<INotificationService>();
        service.Setup(s => s.GetMySummaryAsync(userId))
            .ReturnsAsync(new NotificationSummaryDto { TotalCount = 3, UnreadCount = 1, ReadCount = 2 });
        var controller = new NotificationsController(service.Object, NotificationTestFactory.CurrentUser(userId).Object);

        var result = await controller.GetSummary();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task MarkAsRead_ShouldReturnNoContent()
    {
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var service = new Mock<INotificationService>();
        var controller = new NotificationsController(service.Object, NotificationTestFactory.CurrentUser(userId).Object);

        var result = await controller.MarkAsRead(notificationId);

        var noContent = result.Should().BeOfType<StatusCodeResult>().Subject;
        noContent.StatusCode.Should().Be(204);
        service.Verify(s => s.MarkAsReadAsync(userId, notificationId), Times.Once);
    }

    [Test]
    public async Task MarkAllAsRead_ShouldReturnNoContent()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<INotificationService>();
        var controller = new NotificationsController(service.Object, NotificationTestFactory.CurrentUser(userId).Object);

        var result = await controller.MarkAllAsRead();

        var noContent = result.Should().BeOfType<StatusCodeResult>().Subject;
        noContent.StatusCode.Should().Be(204);
        service.Verify(s => s.MarkAllAsReadAsync(userId), Times.Once);
    }

    [Test]
    public async Task DeleteNotification_ShouldReturnNoContent()
    {
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var service = new Mock<INotificationService>();
        var controller = new NotificationsController(service.Object, NotificationTestFactory.CurrentUser(userId).Object);

        var result = await controller.DeleteNotification(notificationId);

        var noContent = result.Should().BeOfType<StatusCodeResult>().Subject;
        noContent.StatusCode.Should().Be(204);
        service.Verify(s => s.DeleteAsync(userId, notificationId), Times.Once);
    }

    [Test]
    public async Task WhenCurrentUserIdMissing_ShouldThrowUnauthorizedAccessException()
    {
        var service = new Mock<INotificationService>();
        var controller = new NotificationsController(service.Object, NotificationTestFactory.CurrentUser(null).Object);

        await controller.Invoking(c => c.GetSummary())
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
