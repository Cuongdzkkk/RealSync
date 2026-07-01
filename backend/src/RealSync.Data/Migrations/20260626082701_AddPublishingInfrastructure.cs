using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealSync.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPublishingInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConnectedAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChannelType = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExternalAccountId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExternalParentAccountId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProfileUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    GrantedScopesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastValidatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectedAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelType = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "vi"),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Caption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HashtagsJson = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CallToAction = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    SourceGenerationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentVariants_AIContentGenerations_SourceGenerationId",
                        column: x => x.SourceGenerationId,
                        principalTable: "AIContentGenerations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContentVariants_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CredentialType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SecretReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    AllowedCapabilitiesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BudgetDaily = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    BudgetMonthly = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    QuotaMetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastHealthCheckAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSuccessAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastFailureAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureCount = table.Column<int>(type: "int", nullable: false),
                    DisabledReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderCredentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OAuthCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectedAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SecretReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EncryptionKeyVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AccessTokenEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefreshExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CredentialStatus = table.Column<int>(type: "int", nullable: false),
                    LastRefreshAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastRefreshError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OAuthCredentials_ConnectedAccounts_ConnectedAccountId",
                        column: x => x.ConnectedAccountId,
                        principalTable: "ConnectedAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicationJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectedAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PublishMode = table.Column<int>(type: "int", nullable: false),
                    ScheduledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PayloadSnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaManifestJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalPostId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExternalPublishId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PublishedUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemoteStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetryCount = table.Column<int>(type: "int", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastErrorCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LegacyUnverified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationJobs_ConnectedAccounts_ConnectedAccountId",
                        column: x => x.ConnectedAccountId,
                        principalTable: "ConnectedAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PublicationJobs_ContentVariants_ContentVariantId",
                        column: x => x.ContentVariantId,
                        principalTable: "ContentVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PublicationJobs_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderUsageDaily",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderCredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsageDate = table.Column<DateTime>(type: "date", nullable: false),
                    RequestCount = table.Column<int>(type: "int", nullable: false),
                    SuccessCount = table.Column<int>(type: "int", nullable: false),
                    FailureCount = table.Column<int>(type: "int", nullable: false),
                    InputTokens = table.Column<long>(type: "bigint", nullable: false),
                    OutputTokens = table.Column<long>(type: "bigint", nullable: false),
                    GeneratedVideoSeconds = table.Column<long>(type: "bigint", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RateLimitCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderUsageDaily", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderUsageDaily_ProviderCredentials_ProviderCredentialId",
                        column: x => x.ProviderCredentialId,
                        principalTable: "ProviderCredentials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicationAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicationJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    ProviderHttpStatus = table.Column<int>(type: "int", nullable: true),
                    ProviderErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NormalizedErrorCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProviderRequestId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequestMetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseMetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    RetryDecision = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationAttempts_PublicationJobs_PublicationJobId",
                        column: x => x.PublicationJobId,
                        principalTable: "PublicationJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectedAccounts_ExternalAccountId",
                table: "ConnectedAccounts",
                column: "ExternalAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectedAccounts_WorkspaceId_Provider_Status",
                table: "ConnectedAccounts",
                columns: new[] { "WorkspaceId", "Provider", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentVariants_PostId",
                table: "ContentVariants",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentVariants_PostId_ChannelType_Version",
                table: "ContentVariants",
                columns: new[] { "PostId", "ChannelType", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentVariants_SourceGenerationId",
                table: "ContentVariants",
                column: "SourceGenerationId");

            migrationBuilder.CreateIndex(
                name: "IX_OAuthCredentials_ConnectedAccountId",
                table: "OAuthCredentials",
                column: "ConnectedAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderCredentials_WorkspaceId_Provider_Status",
                table: "ProviderCredentials",
                columns: new[] { "WorkspaceId", "Provider", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderUsageDaily_ProviderCredentialId_UsageDate",
                table: "ProviderUsageDaily",
                columns: new[] { "ProviderCredentialId", "UsageDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationAttempts_PublicationJobId_AttemptNumber",
                table: "PublicationAttempts",
                columns: new[] { "PublicationJobId", "AttemptNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationJobs_ConnectedAccountId_CreatedAt",
                table: "PublicationJobs",
                columns: new[] { "ConnectedAccountId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationJobs_ContentVariantId",
                table: "PublicationJobs",
                column: "ContentVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationJobs_IdempotencyKey",
                table: "PublicationJobs",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicationJobs_PostId",
                table: "PublicationJobs",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationJobs_Status_ScheduledAtUtc",
                table: "PublicationJobs",
                columns: new[] { "Status", "ScheduledAtUtc" });

            migrationBuilder.Sql(@"
CREATE TABLE #LegacyVariantMap (
    PostChannelId UNIQUEIDENTIFIER PRIMARY KEY,
    ContentVariantId UNIQUEIDENTIFIER NOT NULL
);

INSERT INTO #LegacyVariantMap (PostChannelId, ContentVariantId)
SELECT Id, NEWID() FROM [PostChannels];

INSERT INTO [ContentVariants] (
    [Id], [PostId], [ChannelType], [Language], [Title], [Caption], [Summary], 
    [Status], [Version], [CreatedAt], [IsDeleted]
)
SELECT 
    m.[ContentVariantId],
    pc.[PostId],
    CASE 
        WHEN pc.[Channel] = 'Website' THEN 0
        WHEN pc.[Channel] = 'Facebook' THEN 1
        WHEN pc.[Channel] = 'Zalo' THEN 5
        ELSE 7
    END AS [ChannelType],
    'vi' AS [Language],
    COALESCE(p.[Title], 'Legacy Post') AS [Title],
    p.[Content] AS [Caption],
    p.[Summary] AS [Summary],
    'Approved' AS [Status],
    1 AS [Version],
    pc.[CreatedAt],
    0 AS [IsDeleted]
FROM [PostChannels] pc
INNER JOIN #LegacyVariantMap m ON pc.[Id] = m.[PostChannelId]
INNER JOIN [Posts] p ON pc.[PostId] = p.[Id];

INSERT INTO [PublicationJobs] (
    [Id], [PostId], [ContentVariantId], [PublishMode], [Status], [Priority],
    [IdempotencyKey], [PublishedUrl], [PublishedAt], [LastErrorMessage],
    [LegacyUnverified], [CreatedAt], [IsDeleted], [RetryCount], [MaxRetryCount]
)
SELECT 
    pc.[Id],
    pc.[PostId],
    m.[ContentVariantId],
    0 AS [PublishMode],
    CASE 
        WHEN pc.[PublishStatus] = 'Pending' THEN 0
        WHEN pc.[PublishStatus] = 'Publishing' THEN 3
        WHEN pc.[PublishStatus] = 'Published' THEN 5
        WHEN pc.[PublishStatus] = 'Failed' THEN 8
        ELSE 0
    END AS [Status],
    0 AS [Priority],
    LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(pc.[Id] AS VARCHAR(36))), 2)) AS [IdempotencyKey],
    pc.[PublishedUrl],
    pc.[PublishedAt],
    pc.[ErrorMessage],
    CASE WHEN pc.[PublishStatus] = 'Published' THEN 1 ELSE 0 END AS [LegacyUnverified],
    pc.[CreatedAt],
    pc.[IsDeleted],
    0 AS [RetryCount],
    5 AS [MaxRetryCount]
FROM [PostChannels] pc
INNER JOIN #LegacyVariantMap m ON pc.[Id] = m.[PostChannelId];

DROP TABLE #LegacyVariantMap;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OAuthCredentials");

            migrationBuilder.DropTable(
                name: "ProviderUsageDaily");

            migrationBuilder.DropTable(
                name: "PublicationAttempts");

            migrationBuilder.DropTable(
                name: "ProviderCredentials");

            migrationBuilder.DropTable(
                name: "PublicationJobs");

            migrationBuilder.DropTable(
                name: "ConnectedAccounts");

            migrationBuilder.DropTable(
                name: "ContentVariants");
        }
    }
}
