#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace_System.Migrations
{
    public partial class AddOrderManagementFlow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Orders",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<DateTime>(name: "PaidAt", table: "Orders", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "PreparingAt", table: "Orders", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "ReadyForPickupAt", table: "Orders", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "CompletedAt", table: "Orders", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "CancelledAt", table: "Orders", type: "datetime2", nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UpdatedAt",
                table: "Orders",
                column: "UpdatedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Orders_Status", table: "Orders");
            migrationBuilder.DropIndex(name: "IX_Orders_UpdatedAt", table: "Orders");

            migrationBuilder.DropColumn(name: "UpdatedAt", table: "Orders");
            migrationBuilder.DropColumn(name: "Notes", table: "Orders");
            migrationBuilder.DropColumn(name: "PaidAt", table: "Orders");
            migrationBuilder.DropColumn(name: "PreparingAt", table: "Orders");
            migrationBuilder.DropColumn(name: "ReadyForPickupAt", table: "Orders");
            migrationBuilder.DropColumn(name: "CompletedAt", table: "Orders");
            migrationBuilder.DropColumn(name: "CancelledAt", table: "Orders");
        }
    }
}