using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Data.Repositories;
using RealSync.Services.Implementations;

namespace RealSync.UnitTests.Leads;

internal static class LeadTestFactory
{
    public static RealSyncDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealSyncDbContext>()
            .UseInMemoryDatabase($"realsync-leads-{Guid.NewGuid():N}")
            .Options;

        return new RealSyncDbContext(options);
    }

    public static LeadService CreateService(RealSyncDbContext context)
    {
        var activityLog = new Mock<IActivityLogService>();

        return new LeadService(
            new LeadRepository(context),
            context,
            activityLog.Object,
            NullLogger<LeadService>.Instance);
    }

    public static User ActiveUser(Guid? id = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            FullName = "Active Agent",
            Email = $"active-{Guid.NewGuid():N}@realsync.vn",
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
            Email = $"inactive-{Guid.NewGuid():N}@realsync.vn",
            PasswordHash = "hash",
            RoleId = Guid.NewGuid(),
            IsActive = false
        };
    }

    public static Property Property(Guid? id = null)
    {
        return new Property
        {
            Id = id ?? Guid.NewGuid(),
            PropertyCode = $"P-{Guid.NewGuid():N}"[..12],
            Title = "Sample Property",
            PropertyTypeId = Guid.NewGuid(),
            Area_ = 80,
            Price = 2_000_000_000
        };
    }

    public static Lead Lead(string fullName, string status = "New", string priority = "Normal", int score = 0)
    {
        return new Lead
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Phone = "0901234567",
            Email = $"{fullName.Replace(" ", string.Empty).ToLowerInvariant()}@example.com",
            Status = status,
            Priority = priority,
            Score = score,
            CreatedAt = DateTime.UtcNow.AddMinutes(-score)
        };
    }
}
