using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealSync.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowUpReminderDispatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FollowUpReminderDispatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledFor = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUpReminderDispatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FollowUpReminderDispatches_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FollowUpReminderDispatches_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpReminderDispatches_LeadId_ScheduledFor",
                table: "FollowUpReminderDispatches",
                columns: new[] { "LeadId", "ScheduledFor" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpReminderDispatches_NotificationId",
                table: "FollowUpReminderDispatches",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpReminderDispatches_ScheduledFor",
                table: "FollowUpReminderDispatches",
                column: "ScheduledFor");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpReminderDispatches_SentAt",
                table: "FollowUpReminderDispatches",
                column: "SentAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowUpReminderDispatches");
        }
    }
}
