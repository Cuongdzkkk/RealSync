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
    public DbSet<PropertyCategory> PropertyCategories => Set<PropertyCategory>();
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

    // Authorization
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // CRM
    public DbSet<Customer> Customers => Set<Customer>();

    // System
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<FollowUpReminderDispatch> FollowUpReminderDispatches => Set<FollowUpReminderDispatch>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    // Posting
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostChannel> PostChannels => Set<PostChannel>();
    public DbSet<PostAnalytics> PostAnalytics => Set<PostAnalytics>();
    public DbSet<PostSchedule> PostSchedules => Set<PostSchedule>();
    public DbSet<AIContentGeneration> AIContentGenerations => Set<AIContentGeneration>();

    // Files
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

    // Publishing and Credentials Infrastructure
    public DbSet<ConnectedAccount> ConnectedAccounts => Set<ConnectedAccount>();
    public DbSet<OAuthCredential> OAuthCredentials => Set<OAuthCredential>();
    public DbSet<ContentVariant> ContentVariants => Set<ContentVariant>();
    public DbSet<PublicationJob> PublicationJobs => Set<PublicationJob>();
    public DbSet<PublicationAttempt> PublicationAttempts => Set<PublicationAttempt>();
    public DbSet<ProviderCredential> ProviderCredentials => Set<ProviderCredential>();
    public DbSet<ProviderUsageDaily> ProviderUsageDailies => Set<ProviderUsageDaily>();
    public DbSet<VideoProject> VideoProjects => Set<VideoProject>();
    public DbSet<VideoScene> VideoScenes => Set<VideoScene>();
    public DbSet<VideoGenerationJob> VideoGenerationJobs => Set<VideoGenerationJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply tất cả configurations từ assembly này
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RealSyncDbContext).Assembly);

        // Global query filter: tự động loại bỏ soft-deleted entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType) && 
                entityType.ClrType != typeof(Permission) && 
                entityType.ClrType != typeof(Role))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
                var lambda = System.Linq.Expressions.Expression.Lambda(condition, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
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

