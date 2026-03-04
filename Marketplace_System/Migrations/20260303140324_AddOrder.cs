using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Marketplace_System.Data;

#nullable disable

namespace Marketplace_System.Migrations
{
    public partial class AddOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF COL_LENGTH('Orders', 'UpdatedAt') IS NULL ALTER TABLE [Orders] ADD [UpdatedAt] datetime2 NOT NULL CONSTRAINT [DF_Orders_UpdatedAt] DEFAULT (SYSUTCDATETIME());");
            migrationBuilder.Sql("IF COL_LENGTH('Orders', 'Notes') IS NULL ALTER TABLE [Orders] ADD [Notes] nvarchar(300) NOT NULL CONSTRAINT [DF_Orders_Notes] DEFAULT (N'');");
            migrationBuilder.Sql("IF COL_LENGTH('Orders', 'PaidAt') IS NULL ALTER TABLE [Orders] ADD [PaidAt] datetime2 NULL;");
            migrationBuilder.Sql("IF COL_LENGTH('Orders', 'PreparingAt') IS NULL ALTER TABLE [Orders] ADD [PreparingAt] datetime2 NULL;");
            migrationBuilder.Sql("IF COL_LENGTH('Orders', 'ReadyForPickupAt') IS NULL ALTER TABLE [Orders] ADD [ReadyForPickupAt] datetime2 NULL;");
            migrationBuilder.Sql("IF COL_LENGTH('Orders', 'CompletedAt') IS NULL ALTER TABLE [Orders] ADD [CompletedAt] datetime2 NULL;");
            migrationBuilder.Sql("IF COL_LENGTH('Orders', 'CancelledAt') IS NULL ALTER TABLE [Orders] ADD [CancelledAt] datetime2 NULL;");

            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Status' AND object_id = OBJECT_ID(N'[Orders]')) CREATE INDEX [IX_Orders_Status] ON [Orders] ([Status]);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_UpdatedAt' AND object_id = OBJECT_ID(N'[Orders]')) CREATE INDEX [IX_Orders_UpdatedAt] ON [Orders] ([UpdatedAt]);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Status' AND object_id = OBJECT_ID(N'[Orders]')) DROP INDEX [IX_Orders_Status] ON [Orders];");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_UpdatedAt' AND object_id = OBJECT_ID(N'[Orders]')) DROP INDEX [IX_Orders_UpdatedAt] ON [Orders];");

            migrationBuilder.Sql("IF COL_LENGTH('Orders', 'UpdatedAt') IS NOT NULL BEGIN ALTER TABLE [Orders] DROP CONSTRAINT IF EXISTS [DF_Orders_UpdatedAt]; ALTER TABLE [Orders] DROP COLUMN [UpdatedAt]; END");
            migrationBuilder.Sql("IF COL_LENGTH('Orders', 'Notes') IS NOT NULL BEGIN ALTER TABLE [Orders] DROP CONSTRAINT IF EXISTS [DF_Orders_Notes]; ALTER TABLE [Orders] DROP COLUMN [Notes]; END");
            migrationBuilder.Sql("IF COL_LENGTH('Orders', 'PaidAt') IS NOT NULL ALTER TABLE [Orders] DROP COLUMN [PaidAt];");
            migrationBuilder.Sql("IF COL_LENGTH('Orders', 'PreparingAt') IS NOT NULL ALTER TABLE [Orders] DROP COLUMN [PreparingAt];");
            migrationBuilder.Sql("IF COL_LENGTH('Orders', 'ReadyForPickupAt') IS NOT NULL ALTER TABLE [Orders] DROP COLUMN [ReadyForPickupAt];");
            migrationBuilder.Sql("IF COL_LENGTH('Orders', 'CompletedAt') IS NOT NULL ALTER TABLE [Orders] DROP COLUMN [CompletedAt];");
            migrationBuilder.Sql("IF COL_LENGTH('Orders', 'CancelledAt') IS NOT NULL ALTER TABLE [Orders] DROP COLUMN [CancelledAt];");
        }
    }
}
