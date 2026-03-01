using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace_System.Migrations
{
    /// <inheritdoc />
    public partial class AddCartItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SellerName",
                table: "ProductListings",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SellerUserId",
                table: "ProductListings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    QuantityKilos = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BuyerUserId = table.Column<int>(type: "int", nullable: false),
                    SellerUserId = table.Column<int>(type: "int", nullable: false),
                    ProductListingId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductListings_SellerUserId",
                table: "ProductListings",
                column: "SellerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_BuyerUserId",
                table: "CartItems",
                column: "BuyerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductListingId",
                table: "CartItems",
                column: "ProductListingId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_SellerUserId",
                table: "CartItems",
                column: "SellerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductListings_SellerUserId",
                table: "ProductListings");

            migrationBuilder.DropColumn(
                name: "SellerName",
                table: "ProductListings");

            migrationBuilder.DropColumn(
                name: "SellerUserId",
                table: "ProductListings");
        }
    }
}
