using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealSync.Api.Controllers;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Leads;
using RealSync.Shared.DTOs.Responses;
using RealSync.Shared.DTOs.Responses.Leads;

namespace RealSync.UnitTests.Leads;

[TestFixture]
public class LeadsControllerTests
{
    [Test]
    public async Task GetLeads_ShouldReturnPagedResponse()
    {
        var service = new Mock<ILeadService>();
        service.Setup(s => s.GetLeadsAsync(It.IsAny<LeadQueryDto>()))
            .ReturnsAsync((new List<LeadListItemDto> { new() { Id = Guid.NewGuid(), FullName = "Lead" } }, 1));
        var controller = new LeadsController(service.Object);

        var result = await controller.GetLeads(new LeadQueryDto { Page = 1, PageSize = 20 });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<IReadOnlyList<LeadListItemDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Meta!.TotalCount.Should().Be(1);
    }

    [Test]
    public async Task GetLead_ShouldReturnOkResponse()
    {
        var id = Guid.NewGuid();
        var service = new Mock<ILeadService>();
        service.Setup(s => s.GetLeadByIdAsync(id))
            .ReturnsAsync(new LeadDetailDto { Id = id, FullName = "Lead" });
        var controller = new LeadsController(service.Object);

        var result = await controller.GetLead(id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task CreateLead_ShouldReturnCreatedResponse()
    {
        var service = new Mock<ILeadService>();
        service.Setup(s => s.CreateLeadAsync(It.IsAny<LeadCreateDto>()))
            .ReturnsAsync(new LeadResponseDto { Id = Guid.NewGuid(), FullName = "Lead" });
        var controller = new LeadsController(service.Object);

        var result = await controller.CreateLead(new LeadCreateDto { FullName = "Lead", Phone = "1" });

        var created = result.Should().BeOfType<ObjectResult>().Subject;
        created.StatusCode.Should().Be(201);
    }

    [Test]
    public async Task UpdateLead_ShouldReturnOkResponse()
    {
        var id = Guid.NewGuid();
        var service = new Mock<ILeadService>();
        service.Setup(s => s.UpdateLeadAsync(id, It.IsAny<LeadUpdateDto>()))
            .ReturnsAsync(new LeadResponseDto { Id = id, FullName = "Lead" });
        var controller = new LeadsController(service.Object);

        var result = await controller.UpdateLead(id, new LeadUpdateDto { FullName = "Lead" });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task DeleteLead_ShouldReturnNoContent()
    {
        var id = Guid.NewGuid();
        var service = new Mock<ILeadService>();
        var controller = new LeadsController(service.Object);

        var result = await controller.DeleteLead(id);

        var noContent = result.Should().BeOfType<StatusCodeResult>().Subject;
        noContent.StatusCode.Should().Be(204);
        service.Verify(s => s.DeleteLeadAsync(id), Times.Once);
    }

    [Test]
    public async Task UpdateStatus_ShouldReturnOkResponse()
    {
        var id = Guid.NewGuid();
        var service = new Mock<ILeadService>();
        service.Setup(s => s.UpdateStatusAsync(id, It.IsAny<LeadStatusUpdateDto>()))
            .ReturnsAsync(new LeadResponseDto { Id = id, Status = "Contacted" });
        var controller = new LeadsController(service.Object);

        var result = await controller.UpdateStatus(id, new LeadStatusUpdateDto { Status = "Contacted" });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task AssignLead_ShouldReturnOkResponse()
    {
        var id = Guid.NewGuid();
        var service = new Mock<ILeadService>();
        service.Setup(s => s.AssignLeadAsync(id, It.IsAny<LeadAssignDto>()))
            .ReturnsAsync(new LeadResponseDto { Id = id, AssignedToId = Guid.NewGuid() });
        var controller = new LeadsController(service.Object);

        var result = await controller.AssignLead(id, new LeadAssignDto { AssignedToId = Guid.NewGuid() });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task AddActivity_ShouldReturnCreatedResponse()
    {
        var id = Guid.NewGuid();
        var service = new Mock<ILeadService>();
        service.Setup(s => s.AddActivityAsync(id, It.IsAny<LeadActivityCreateDto>()))
            .ReturnsAsync(new LeadActivityDto { Id = Guid.NewGuid(), ActivityType = "Call" });
        var controller = new LeadsController(service.Object);

        var result = await controller.AddActivity(id, new LeadActivityCreateDto { ActivityType = "Call" });

        var created = result.Should().BeOfType<ObjectResult>().Subject;
        created.StatusCode.Should().Be(201);
    }

    [Test]
    public async Task GetActivities_ShouldReturnOkResponse()
    {
        var id = Guid.NewGuid();
        var service = new Mock<ILeadService>();
        service.Setup(s => s.GetActivitiesAsync(id))
            .ReturnsAsync(new List<LeadActivityDto> { new() { Id = Guid.NewGuid(), ActivityType = "Call" } });
        var controller = new LeadsController(service.Object);

        var result = await controller.GetActivities(id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task SetFollowUp_ShouldReturnOkResponse()
    {
        var id = Guid.NewGuid();
        var service = new Mock<ILeadService>();
        service.Setup(s => s.SetFollowUpAsync(id, It.IsAny<LeadFollowUpDto>()))
            .ReturnsAsync(new LeadResponseDto { Id = id, NextFollowUpAt = DateTime.UtcNow.AddDays(1) });
        var controller = new LeadsController(service.Object);

        var result = await controller.SetFollowUp(id, new LeadFollowUpDto { NextFollowUpAt = DateTime.UtcNow.AddDays(1) });

        result.Should().BeOfType<OkObjectResult>();
    }
}
