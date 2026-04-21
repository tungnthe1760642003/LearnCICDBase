using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace learnnet.Migrations
{
    /// <inheritdoc />
    public partial class AdvancedFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_Status",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Status",
                table: "Tasks",
                column: "Status",
                filter: "\"Status\" != 3");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Status_Priority",
                table: "Tasks",
                columns: new[] { "Status", "Priority" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_Status",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Status_Priority",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Status",
                table: "Tasks",
                column: "Status");
        }
    }
}
