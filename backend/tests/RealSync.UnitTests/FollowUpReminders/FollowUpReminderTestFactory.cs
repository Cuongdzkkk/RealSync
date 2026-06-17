using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RealSync.Core.Entities;
using RealSync.Data.Context;
using RealSync.Services.Implementations;
using RealSync.Services.Options;

namespace RealSync.UnitTests.FollowUpReminders;

internal sealed class SqliteReminderContext : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteReminderContext()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<RealSyncDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new RealSyncDbContext(options);
        CreateSchema(Context);
    }

    public RealSyncDbContext Context { get; }

    private static void CreateSchema(RealSyncDbContext context)
    {
        context.Database.ExecuteSqlRaw("""
            CREATE TABLE Roles (
                Id TEXT NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
                Name TEXT NOT NULL,
                Description TEXT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NULL,
                CreatedBy TEXT NULL,
                UpdatedBy TEXT NULL,
                IsDeleted INTEGER NOT NULL,
                DeletedAt TEXT NULL
            );
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE TABLE Users (
                Id TEXT NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
                FullName TEXT NOT NULL,
                Email TEXT NOT NULL,
                PasswordHash TEXT NOT NULL,
                Phone TEXT NULL,
                AvatarUrl TEXT NULL,
                IsActive INTEGER NOT NULL,
                LastLoginAt TEXT NULL,
                RoleId TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NULL,
                CreatedBy TEXT NULL,
                UpdatedBy TEXT NULL,
                IsDeleted INTEGER NOT NULL,
                DeletedAt TEXT NULL,
                CONSTRAINT FK_Users_Roles_RoleId FOREIGN KEY (RoleId) REFERENCES Roles (Id) ON DELETE RESTRICT
            );
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE TABLE Leads (
                Id TEXT NOT NULL CONSTRAINT PK_Leads PRIMARY KEY,
                FullName TEXT NOT NULL,
                Phone TEXT NULL,
                Email TEXT NULL,
                Status TEXT NOT NULL,
                Priority TEXT NOT NULL,
                Score INTEGER NOT NULL,
                InterestedPropertyId TEXT NULL,
                Budget TEXT NULL,
                Requirements TEXT NULL,
                PreferredArea TEXT NULL,
                PreferredType TEXT NULL,
                AssignedToId TEXT NULL,
                SourceChannel TEXT NULL,
                LastContactedAt TEXT NULL,
                NextFollowUpAt TEXT NULL,
                ConvertedAt TEXT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NULL,
                CreatedBy TEXT NULL,
                UpdatedBy TEXT NULL,
                IsDeleted INTEGER NOT NULL,
                DeletedAt TEXT NULL
            );
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE TABLE Notifications (
                Id TEXT NOT NULL CONSTRAINT PK_Notifications PRIMARY KEY,
                UserId TEXT NOT NULL,
                Title TEXT NOT NULL,
                Message TEXT NOT NULL,
                Type TEXT NOT NULL,
                IsRead INTEGER NOT NULL,
                ReadAt TEXT NULL,
                Data TEXT NULL,
                Link TEXT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NULL,
                CreatedBy TEXT NULL,
                UpdatedBy TEXT NULL,
                IsDeleted INTEGER NOT NULL,
                DeletedAt TEXT NULL,
                CONSTRAINT FK_Notifications_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
            );
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE TABLE FollowUpReminderDispatches (
                Id TEXT NOT NULL CONSTRAINT PK_FollowUpReminderDispatches PRIMARY KEY,
                LeadId TEXT NOT NULL,
                ScheduledFor TEXT NOT NULL,
                NotificationId TEXT NOT NULL,
                SentAt TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NULL,
                CreatedBy TEXT NULL,
                UpdatedBy TEXT NULL,
                IsDeleted INTEGER NOT NULL,
                DeletedAt TEXT NULL,
                CONSTRAINT FK_FollowUpReminderDispatches_Leads_LeadId FOREIGN KEY (LeadId) REFERENCES Leads (Id) ON DELETE NO ACTION,
                CONSTRAINT FK_FollowUpReminderDispatches_Notifications_NotificationId FOREIGN KEY (NotificationId) REFERENCES Notifications (Id) ON DELETE NO ACTION
            );
            """);

        context.Database.ExecuteSqlRaw("CREATE UNIQUE INDEX IX_FollowUpReminderDispatches_LeadId_ScheduledFor ON FollowUpReminderDispatches (LeadId, ScheduledFor);");
        context.Database.ExecuteSqlRaw("CREATE INDEX IX_FollowUpReminderDispatches_ScheduledFor ON FollowUpReminderDispatches (ScheduledFor);");
        context.Database.ExecuteSqlRaw("CREATE INDEX IX_FollowUpReminderDispatches_SentAt ON FollowUpReminderDispatches (SentAt);");
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}

internal static class FollowUpReminderTestFactory
{
    public static SqliteReminderContext CreateSqliteContext()
    {
        return new SqliteReminderContext();
    }

    public static FollowUpReminderService CreateService(
        RealSyncDbContext context,
        int batchSize = 100)
    {
        return new FollowUpReminderService(
            context,
            Options.Create(new FollowUpReminderOptions { BatchSize = batchSize }),
            NullLogger<FollowUpReminderService>.Instance);
    }

    public static Role Role()
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = $"Role-{Guid.NewGuid():N}"
        };
    }

    public static User User(Guid roleId)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FullName = "Reminder Agent",
            Email = $"reminder-{Guid.NewGuid():N}@realsync.vn",
            PasswordHash = "hash",
            RoleId = roleId,
            IsActive = true
        };
    }

    public static Lead Lead(
        Guid? assignedToId,
        DateTime? nextFollowUpAt,
        string status = "Contacted",
        bool isDeleted = false)
    {
        return new Lead
        {
            Id = Guid.NewGuid(),
            FullName = $"Reminder Lead {Guid.NewGuid():N}",
            Phone = "0901234567",
            Status = status,
            AssignedToId = assignedToId,
            NextFollowUpAt = nextFollowUpAt,
            IsDeleted = isDeleted
        };
    }
}
