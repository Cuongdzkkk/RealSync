using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;

namespace RealSync.Data.Context;

/// <summary>
/// DbContext chính của RealSync.
/// Auto-fill audit fields (CreatedBy, UpdatedBy) từ ICurrentUserService.
/// </summary>
public class RealSyncDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public RealSyncDbContext(DbContextOptions<RealSyncDbContext> options)
        : base(options)
    {
    }

    public RealSyncDbContext(
        DbContextOptions<RealSyncDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    // Core
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Area> Areas => Set<Area>();

    // Leads
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadActivity> LeadActivities => Set<LeadActivity>();

    // Users
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();

    // Crawler
    public DbSet<CrawlSource> CrawlSources => Set<CrawlSource>();
    public DbSet<CrawlJob> CrawlJobs => Set<CrawlJob>();
    public DbSet<CrawlResult> CrawlResults => Set<CrawlResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply tất cả configurations từ assembly này
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RealSyncDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditFields();
        return base.SaveChanges();
    }

    private void ApplyAuditFields()
    {
        var currentUser = _currentUserService?.Email ?? "system";
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy ??= currentUser;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUser;
                    break;
            }
        }
    }
}

