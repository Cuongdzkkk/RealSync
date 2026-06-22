using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealSync.Api.Controllers;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.CrmAnalytics;
using RealSync.Shared.DTOs.Responses.CrmAnalytics;

namespace RealSync.UnitTests.CrmAnalytics;

[TestFixture]
public class CrmAnalyticsControllerTests
{
    [Test]
    public async Task GetSummary_ShouldReturnOkResponse()
    {
        var service = new Mock<ICrmAnalyticsService>();
        service.Setup(s => s.GetSummaryAsync(It.IsAny<CrmAnalyticsQueryDto>()))
            .ReturnsAsync(new CrmAnalyticsSummaryDto { TotalLeads = 1 });
        var controller = new CrmAnalyticsController(service.Object);

        var result = await controller.GetSummary(new CrmAnalyticsQueryDto());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task GetLeadsByStatus_ShouldReturnOkResponse()
    {
        var service = new Mock<ICrmAnalyticsService>();
        service.Setup(s => s.GetLeadsByStatusAsync(It.IsAny<CrmAnalyticsQueryDto>()))
            .ReturnsAsync(new List<CrmCountByLabelDto> { new() { Label = "New", Count = 1 } });
        var controller = new CrmAnalyticsController(service.Object);

        var result = await controller.GetLeadsByStatus(new CrmAnalyticsQueryDto());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task GetLeadsBySource_ShouldReturnOkResponse()
    {
        var service = new Mock<ICrmAnalyticsService>();
        service.Setup(s => s.GetLeadsBySourceAsync(It.IsAny<CrmAnalyticsQueryDto>()))
            .ReturnsAsync(new List<CrmCountByLabelDto> { new() { Label = "Website", Count = 1 } });
        var controller = new CrmAnalyticsController(service.Object);

        var result = await controller.GetLeadsBySource(new CrmAnalyticsQueryDto());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task GetConversion_ShouldReturnOkResponse()
    {
        var service = new Mock<ICrmAnalyticsService>();
        service.Setup(s => s.GetConversionStatsAsync(It.IsAny<CrmAnalyticsQueryDto>()))
            .ReturnsAsync(new CrmConversionStatsDto { TotalLeads = 1 });
        var controller = new CrmAnalyticsController(service.Object);

        var result = await controller.GetConversion(new CrmAnalyticsQueryDto());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task GetFollowUps_ShouldReturnOkResponse()
    {
        var service = new Mock<ICrmAnalyticsService>();
        service.Setup(s => s.GetFollowUpStatsAsync(It.IsAny<CrmAnalyticsQueryDto>()))
            .ReturnsAsync(new CrmFollowUpStatsDto { TotalLeadsWithFollowUp = 1 });
        var controller = new CrmAnalyticsController(service.Object);

        var result = await controller.GetFollowUps(new CrmAnalyticsQueryDto());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task GetCustomers_ShouldReturnOkResponse()
    {
        var service = new Mock<ICrmAnalyticsService>();
        service.Setup(s => s.GetCustomerStatsAsync(It.IsAny<CrmAnalyticsQueryDto>()))
            .ReturnsAsync(new CrmCustomerStatsDto { TotalCustomers = 1 });
        var controller = new CrmAnalyticsController(service.Object);

        var result = await controller.GetCustomers(new CrmAnalyticsQueryDto());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task GetMonthlyTrend_ShouldReturnOkResponse()
    {
        var service = new Mock<ICrmAnalyticsService>();
        service.Setup(s => s.GetMonthlyTrendAsync(2026))
            .ReturnsAsync(new List<CrmMonthlyTrendDto> { new() { Month = "Jan" } });
        var controller = new CrmAnalyticsController(service.Object);

        var result = await controller.GetMonthlyTrend(2026);

        result.Should().BeOfType<OkObjectResult>();
    }
}
