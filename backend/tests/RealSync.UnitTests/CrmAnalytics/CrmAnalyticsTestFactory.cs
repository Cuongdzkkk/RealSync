using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RealSync.Core.Entities;
using RealSync.Data.Context;
using RealSync.Services.Implementations;

namespace RealSync.UnitTests.CrmAnalytics;

internal static class CrmAnalyticsTestFactory
{
    public static RealSyncDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealSyncDbContext>()
            .UseInMemoryDatabase($"realsync-crm-analytics-{Guid.NewGuid():N}")
            .Options;

        return new RealSyncDbContext(options);
    }

    public static CrmAnalyticsService CreateService(RealSyncDbContext context)
    {
        return new CrmAnalyticsService(context, NullLogger<CrmAnalyticsService>.Instance);
    }

    public static Lead Lead(
        string fullName,
        string status = "New",
        int score = 0,
        string? sourceChannel = null,
        Guid? assignedToId = null,
        DateTime? followUpAt = null)
    {
        return new Lead
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Phone = "0901234567",
            Email = $"{fullName.Replace(" ", string.Empty).ToLowerInvariant()}@example.com",
            Status = status,
            Score = score,
            SourceChannel = sourceChannel,
            AssignedToId = assignedToId,
            NextFollowUpAt = followUpAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Customer Customer(
        string fullName,
        string? source = null,
        Guid? convertedFromLeadId = null,
        Guid? assignedToId = null)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Phone = "0901234567",
            Email = $"{fullName.Replace(" ", string.Empty).ToLowerInvariant()}@example.com",
            Source = source,
            ConvertedFromLeadId = convertedFromLeadId,
            AssignedToId = assignedToId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
