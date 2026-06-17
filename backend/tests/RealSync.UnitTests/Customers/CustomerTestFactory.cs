using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Data.Repositories;
using RealSync.Services.Implementations;

namespace RealSync.UnitTests.Customers;

internal static class CustomerTestFactory
{
    public static RealSyncDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealSyncDbContext>()
            .UseInMemoryDatabase($"realsync-customers-{Guid.NewGuid():N}")
            .Options;

        return new RealSyncDbContext(options);
    }

    public static CustomerService CreateService(
        RealSyncDbContext context,
        Mock<IActivityLogService>? activityLogService = null)
    {
        activityLogService ??= new Mock<IActivityLogService>();

        return new CustomerService(
            new CustomerRepository(context),
            context,
            activityLogService.Object,
            NullLogger<CustomerService>.Instance);
    }

    public static User ActiveUser(Guid? id = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            FullName = "Active Agent",
            Email = $"customer-active-{Guid.NewGuid():N}@realsync.vn",
            PasswordHash = "hash",
            RoleId = Guid.NewGuid(),
            IsActive = true
        };
    }

    public static User InactiveUser(Guid? id = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            FullName = "Inactive Agent",
            Email = $"customer-inactive-{Guid.NewGuid():N}@realsync.vn",
            PasswordHash = "hash",
            RoleId = Guid.NewGuid(),
            IsActive = false
        };
    }

    public static Customer Customer(string fullName, string? source = null)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Phone = "0901234567",
            Email = $"{fullName.Replace(" ", string.Empty).ToLowerInvariant()}@example.com",
            Source = source,
            CreatedAt = DateTime.UtcNow
        };
    }
}
