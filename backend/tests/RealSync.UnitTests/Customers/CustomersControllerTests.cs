using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealSync.Api.Controllers;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Customers;
using RealSync.Shared.DTOs.Responses;
using RealSync.Shared.DTOs.Responses.Customers;

namespace RealSync.UnitTests.Customers;

[TestFixture]
public class CustomersControllerTests
{
    [Test]
    public async Task GetCustomers_ShouldReturnPagedResponse()
    {
        var service = new Mock<ICustomerService>();
        service.Setup(s => s.GetCustomersAsync(It.IsAny<CustomerQueryDto>()))
            .ReturnsAsync((new List<CustomerListItemDto> { new() { Id = Guid.NewGuid(), FullName = "Customer" } }, 1));
        var controller = new CustomersController(service.Object);

        var result = await controller.GetCustomers(new CustomerQueryDto { Page = 1, PageSize = 20 });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<IReadOnlyList<CustomerListItemDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Meta!.TotalCount.Should().Be(1);
    }

    [Test]
    public async Task GetCustomer_ShouldReturnOkResponse()
    {
        var id = Guid.NewGuid();
        var service = new Mock<ICustomerService>();
        service.Setup(s => s.GetCustomerByIdAsync(id))
            .ReturnsAsync(new CustomerDetailDto { Id = id, FullName = "Customer" });
        var controller = new CustomersController(service.Object);

        var result = await controller.GetCustomer(id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task CreateCustomer_ShouldReturnCreatedResponse()
    {
        var service = new Mock<ICustomerService>();
        service.Setup(s => s.CreateCustomerAsync(It.IsAny<CustomerCreateDto>()))
            .ReturnsAsync(new CustomerResponseDto { Id = Guid.NewGuid(), FullName = "Customer" });
        var controller = new CustomersController(service.Object);

        var result = await controller.CreateCustomer(new CustomerCreateDto { FullName = "Customer", Phone = "1" });

        var created = result.Should().BeOfType<ObjectResult>().Subject;
        created.StatusCode.Should().Be(201);
    }

    [Test]
    public async Task UpdateCustomer_ShouldReturnOkResponse()
    {
        var id = Guid.NewGuid();
        var service = new Mock<ICustomerService>();
        service.Setup(s => s.UpdateCustomerAsync(id, It.IsAny<CustomerUpdateDto>()))
            .ReturnsAsync(new CustomerResponseDto { Id = id, FullName = "Customer" });
        var controller = new CustomersController(service.Object);

        var result = await controller.UpdateCustomer(id, new CustomerUpdateDto { FullName = "Customer" });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task DeleteCustomer_ShouldReturnNoContent()
    {
        var id = Guid.NewGuid();
        var service = new Mock<ICustomerService>();
        var controller = new CustomersController(service.Object);

        var result = await controller.DeleteCustomer(id);

        var noContent = result.Should().BeOfType<StatusCodeResult>().Subject;
        noContent.StatusCode.Should().Be(204);
        service.Verify(s => s.DeleteCustomerAsync(id), Times.Once);
    }

    [Test]
    public async Task GetActivities_ShouldReturnOkResponse()
    {
        var id = Guid.NewGuid();
        var service = new Mock<ICustomerService>();
        service.Setup(s => s.GetCustomerActivitiesAsync(id))
            .ReturnsAsync(new List<CustomerActivityLogDto> { new() { Id = Guid.NewGuid(), EntityType = "Customer" } });
        var controller = new CustomersController(service.Object);

        var result = await controller.GetActivities(id);

        result.Should().BeOfType<OkObjectResult>();
    }
}
