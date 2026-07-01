using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealSync.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoGenerationSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VideoProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TargetChannel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AspectRatio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetDurationSeconds = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApprovedStoryboardVersion = table.Column<int>(type: "int", nullable: false),
                    FinalAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoProjects_ContentVariants_ContentVariantId",
                        column: x => x.ContentVariantId,
                        principalTable: "ContentVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoProjects_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoProjects_StoredFiles_FinalAssetId",
                        column: x => x.FinalAssetId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VideoScenes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VideoProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    OnScreenText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    VisualPrompt = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    NegativePrompt = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CameraDirection = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReferenceAssetIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    GeneratedAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoScenes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoScenes_StoredFiles_GeneratedAssetId",
                        column: x => x.GeneratedAssetId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VideoScenes_VideoProjects_VideoProjectId",
                        column: x => x.VideoProjectId,
                        principalTable: "VideoProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoGenerationJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VideoProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VideoSceneId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OperationId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProgressPercent = table.Column<int>(type: "int", nullable: true),
                    OutputAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ErrorCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoGenerationJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoGenerationJobs_StoredFiles_OutputAssetId",
                        column: x => x.OutputAssetId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VideoGenerationJobs_VideoProjects_VideoProjectId",
                        column: x => x.VideoProjectId,
                        principalTable: "VideoProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoGenerationJobs_VideoScenes_VideoSceneId",
                        column: x => x.VideoSceneId,
                        principalTable: "VideoScenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoGenerationJobs_OutputAssetId",
                table: "VideoGenerationJobs",
                column: "OutputAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoGenerationJobs_Status_OperationId",
                table: "VideoGenerationJobs",
                columns: new[] { "Status", "OperationId" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoGenerationJobs_VideoProjectId",
                table: "VideoGenerationJobs",
                column: "VideoProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoGenerationJobs_VideoSceneId",
                table: "VideoGenerationJobs",
                column: "VideoSceneId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoProjects_ContentVariantId",
                table: "VideoProjects",
                column: "ContentVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoProjects_FinalAssetId",
                table: "VideoProjects",
                column: "FinalAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoProjects_PostId",
                table: "VideoProjects",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoProjects_Status_CreatedAt",
                table: "VideoProjects",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoScenes_GeneratedAssetId",
                table: "VideoScenes",
                column: "GeneratedAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoScenes_VideoProjectId_Sequence",
                table: "VideoScenes",
                columns: new[] { "VideoProjectId", "Sequence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoGenerationJobs");

            migrationBuilder.DropTable(
                name: "VideoScenes");

            migrationBuilder.DropTable(
                name: "VideoProjects");
        }
    }
}
