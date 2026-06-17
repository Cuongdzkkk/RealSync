using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RealSync.Core.Enums;
using RealSync.Data.Repositories;
using RealSync.Shared.DTOs.Requests.Customers;

namespace RealSync.UnitTests.Customers;

[TestFixture]
public class CustomerRepositoryTests
{
    [Test]
    public async Task GetPagedAsync_ShouldApplyFiltersAndSorting()
    {
        await using var context = CustomerTestFactory.CreateContext();
        var assignedUser = CustomerTestFactory.ActiveUser();
        var convertedLead = Guid.NewGuid();
        var alpha = CustomerTestFactory.Customer("Alpha Customer", "Website");
        alpha.AssignedToId = assignedUser.Id;
        alpha.ConvertedFromLeadId = convertedLead;
        alpha.CreatedAt = DateTime.UtcNow.AddDays(-1);
        var beta = CustomerTestFactory.Customer("Beta Customer", "Referral");
        beta.CreatedAt = DateTime.UtcNow;
        context.Users.Add(assignedUser);
        context.Customers.AddRange(alpha, beta, CustomerTestFactory.Customer("Gamma Customer", "Website"));
        await context.SaveChangesAsync();
        var repository = new CustomerRepository(context);

        var paged = await repository.GetPagedAsync(new CustomerQueryDto { Page = 1, PageSize = 2 });
        var search = await repository.GetPagedAsync(new CustomerQueryDto { Search = "Alpha" });
        var source = await repository.GetPagedAsync(new CustomerQueryDto { Source = "Website" });
        var assigned = await repository.GetPagedAsync(new CustomerQueryDto { AssignedToId = assignedUser.Id });
        var converted = await repository.GetPagedAsync(new CustomerQueryDto { ConvertedFromLeadId = convertedLead });
        var sorted = await repository.GetPagedAsync(new CustomerQueryDto { SortBy = "fullName", SortDirection = "asc" });

        paged.Items.Should().HaveCount(2);
        paged.TotalCount.Should().Be(3);
        search.Items.Should().ContainSingle(x => x.Id == alpha.Id);
        source.Items.Should().OnlyContain(x => x.Source == "Website");
        assigned.Items.Should().ContainSingle(x => x.AssignedToId == assignedUser.Id);
        converted.Items.Should().ContainSingle(x => x.ConvertedFromLeadId == convertedLead);
        sorted.Items.Select(x => x.FullName).Should().BeInAscendingOrder();
    }

    [Test]
    public async Task GetDetailByIdAsync_ShouldIncludeAssignedUserAndConvertedLead()
    {
        await using var context = CustomerTestFactory.CreateContext();
        var user = CustomerTestFactory.ActiveUser();
        var lead = RealSync.UnitTests.Leads.LeadTestFactory.Lead("Converted Lead");
        var customer = CustomerTestFactory.Customer("Included Customer");
        customer.AssignedToId = user.Id;
        customer.ConvertedFromLeadId = lead.Id;
        context.Users.Add(user);
        context.Leads.Add(lead);
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var result = await new CustomerRepository(context).GetDetailByIdAsync(customer.Id);

        result.Should().NotBeNull();
        result!.AssignedTo!.FullName.Should().Be(user.FullName);
        result.ConvertedFromLead!.FullName.Should().Be(lead.FullName);
    }

    [Test]
    public async Task DeleteAsync_ShouldSoftDeleteCustomer()
    {
        await using var context = CustomerTestFactory.CreateContext();
        var customer = CustomerTestFactory.Customer("Repo Delete");
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        await new CustomerRepository(context).DeleteAsync(customer);

        var deleted = await context.Customers.IgnoreQueryFilters().FirstAsync(c => c.Id == customer.Id);
        deleted.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().NotBeNull();
    }

    [Test]
    public async Task GetActivityLogsAsync_ShouldReturnCustomerLogsOnlyOrderedDesc()
    {
        await using var context = CustomerTestFactory.CreateContext();
        var customer = CustomerTestFactory.Customer("Activity Customer");
        context.Customers.Add(customer);
        context.ActivityLogs.AddRange(
            new() { EntityType = "Customer", EntityId = customer.Id, Action = ActivityType.Create, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { EntityType = "Customer", EntityId = customer.Id, Action = ActivityType.Update, CreatedAt = DateTime.UtcNow },
            new() { EntityType = "Lead", EntityId = customer.Id, Action = ActivityType.Update, CreatedAt = DateTime.UtcNow.AddDays(1) });
        await context.SaveChangesAsync();

        var logs = await new CustomerRepository(context).GetActivityLogsAsync(customer.Id);

        logs.Should().HaveCount(2);
        logs.Should().OnlyContain(l => l.EntityType == "Customer" && l.EntityId == customer.Id);
        logs.Select(l => l.CreatedAt).Should().BeInDescendingOrder();
    }
}
