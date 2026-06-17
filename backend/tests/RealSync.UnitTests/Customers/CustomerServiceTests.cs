using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Customers;
using RealSync.Shared.Exceptions;
using ValidationException = RealSync.Shared.Exceptions.ValidationException;

namespace RealSync.UnitTests.Customers;

[TestFixture]
public class CustomerServiceTests
{
    [Test]
    public async Task CreateCustomerAsync_ValidRequest_ShouldCreateCustomerAndLog()
    {
        await using var context = CustomerTestFactory.CreateContext();
        var activityLog = new Mock<IActivityLogService>();
        var service = CustomerTestFactory.CreateService(context, activityLog);

        var result = await service.CreateCustomerAsync(new CustomerCreateDto
        {
            FullName = " Nguyen Van A ",
            Email = " customer@example.com ",
            Source = " Website "
        });

        result.FullName.Should().Be("Nguyen Van A");
        result.Email.Should().Be("customer@example.com");
        result.Source.Should().Be("Website");
        (await context.Customers.CountAsync()).Should().Be(1);
        activityLog.Verify(l => l.LogAsync("Customer", result.Id, ActivityType.Create, It.IsAny<string>(), null, It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task CreateCustomerAsync_InvalidContactOrAssignedUser_ShouldThrow()
    {
        await using var context = CustomerTestFactory.CreateContext();
        var inactiveUser = CustomerTestFactory.InactiveUser();
        context.Users.Add(inactiveUser);
        await context.SaveChangesAsync();
        var service = CustomerTestFactory.CreateService(context);

        await service.Invoking(s => s.CreateCustomerAsync(new CustomerCreateDto { FullName = "No Contact" }))
            .Should().ThrowAsync<ValidationException>();

        await service.Invoking(s => s.CreateCustomerAsync(new CustomerCreateDto
        {
            FullName = "Invalid Assigned",
            Phone = "1",
            AssignedToId = inactiveUser.Id
        })).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task GetCustomersAsync_ShouldSearchFilterAndPaginate()
    {
        await using var context = CustomerTestFactory.CreateContext();
        var user = CustomerTestFactory.ActiveUser();
        var leadId = Guid.NewGuid();
        var alpha = CustomerTestFactory.Customer("Alpha Customer", "Website");
        alpha.AssignedToId = user.Id;
        alpha.ConvertedFromLeadId = leadId;
        context.Users.Add(user);
        context.Customers.AddRange(
            alpha,
            CustomerTestFactory.Customer("Beta Customer", "Referral"),
            CustomerTestFactory.Customer("Gamma Customer", "Website"));
        await context.SaveChangesAsync();

        var service = CustomerTestFactory.CreateService(context);
        var paged = await service.GetCustomersAsync(new CustomerQueryDto { Page = 1, PageSize = 2 });
        var search = await service.GetCustomersAsync(new CustomerQueryDto { Search = "Alpha" });
        var source = await service.GetCustomersAsync(new CustomerQueryDto { Source = "Website" });
        var assigned = await service.GetCustomersAsync(new CustomerQueryDto { AssignedToId = user.Id });
        var converted = await service.GetCustomersAsync(new CustomerQueryDto { ConvertedFromLead = true });

        paged.Items.Should().HaveCount(2);
        paged.TotalCount.Should().Be(3);
        search.Items.Should().ContainSingle(x => x.FullName == "Alpha Customer");
        source.Items.Should().OnlyContain(x => x.Source == "Website");
        assigned.Items.Should().ContainSingle(x => x.AssignedToId == user.Id);
        converted.Items.Should().ContainSingle(x => x.ConvertedFromLeadId == leadId);
    }

    [Test]
    public async Task GetCustomerByIdAsync_ExistingCustomer_ShouldReturnDetailWithActivities()
    {
        await using var context = CustomerTestFactory.CreateContext();
        var customer = CustomerTestFactory.Customer("Detail Customer");
        context.Customers.Add(customer);
        context.ActivityLogs.Add(new()
        {
            EntityType = "Customer",
            EntityId = customer.Id,
            Action = ActivityType.Update,
            Description = "Updated",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var result = await CustomerTestFactory.CreateService(context).GetCustomerByIdAsync(customer.Id);

        result.Id.Should().Be(customer.Id);
        result.Activities.Should().ContainSingle(a => a.Action == "Update");

        await CustomerTestFactory.CreateService(context).Invoking(s => s.GetCustomerByIdAsync(Guid.NewGuid()))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task UpdateCustomerAsync_ShouldUpdateAndValidate()
    {
        await using var context = CustomerTestFactory.CreateContext();
        var customer = CustomerTestFactory.Customer("Old Customer");
        var inactiveUser = CustomerTestFactory.InactiveUser();
        context.Customers.Add(customer);
        context.Users.Add(inactiveUser);
        await context.SaveChangesAsync();
        var service = CustomerTestFactory.CreateService(context);

        var updated = await service.UpdateCustomerAsync(customer.Id, new CustomerUpdateDto
        {
            FullName = "New Customer",
            Company = "RealSync"
        });

        updated.FullName.Should().Be("New Customer");
        updated.Company.Should().Be("RealSync");

        await service.Invoking(s => s.UpdateCustomerAsync(Guid.NewGuid(), new CustomerUpdateDto { FullName = "Missing" }))
            .Should().ThrowAsync<NotFoundException>();
        await service.Invoking(s => s.UpdateCustomerAsync(customer.Id, new CustomerUpdateDto { AssignedToId = inactiveUser.Id }))
            .Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task DeleteCustomerAsync_ExistingCustomer_ShouldSoftDelete()
    {
        await using var context = CustomerTestFactory.CreateContext();
        var customer = CustomerTestFactory.Customer("Delete Customer");
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        await CustomerTestFactory.CreateService(context).DeleteCustomerAsync(customer.Id);

        var deleted = await context.Customers.IgnoreQueryFilters().FirstAsync(c => c.Id == customer.Id);
        deleted.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().NotBeNull();

        await CustomerTestFactory.CreateService(context).Invoking(s => s.DeleteCustomerAsync(Guid.NewGuid()))
            .Should().ThrowAsync<NotFoundException>();
    }
}
