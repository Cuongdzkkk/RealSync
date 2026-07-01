using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealSync.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAIUsageTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompletionTokens",
                table: "AIContentGenerations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "AIContentGenerations",
                type: "decimal(18,9)",
                precision: 18,
                scale: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FactsUsedJson",
                table: "AIContentGenerations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PromptTokens",
                table: "AIContentGenerations",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionTokens",
                table: "AIContentGenerations");

            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "AIContentGenerations");

            migrationBuilder.DropColumn(
                name: "FactsUsedJson",
                table: "AIContentGenerations");

            migrationBuilder.DropColumn(
                name: "PromptTokens",
                table: "AIContentGenerations");
        }
    }
}
