using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace_System.Migrations
{
    public partial class AddPayments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[Payments]', N'U') IS NULL
BEGIN
    CREATE TABLE [Payments] (
        [Id] int NOT NULL IDENTITY,
        [ReferenceNumber] nvarchar(30) NOT NULL,
        [OrderNumber] nvarchar(24) NOT NULL,
        [PayerName] nvarchar(120) NOT NULL,
        [RecipientName] nvarchar(120) NOT NULL,
        [Method] nvarchar(24) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Notes] nvarchar(250) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id])
    );
END");

            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Payments_ReferenceNumber' AND object_id = OBJECT_ID(N'[Payments]')) CREATE UNIQUE INDEX [IX_Payments_ReferenceNumber] ON [Payments] ([ReferenceNumber]);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Payments_OrderNumber' AND object_id = OBJECT_ID(N'[Payments]')) CREATE INDEX [IX_Payments_OrderNumber] ON [Payments] ([OrderNumber]);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Payments_Status' AND object_id = OBJECT_ID(N'[Payments]')) CREATE INDEX [IX_Payments_Status] ON [Payments] ([Status]);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Payments_CreatedAt' AND object_id = OBJECT_ID(N'[Payments]')) CREATE INDEX [IX_Payments_CreatedAt] ON [Payments] ([CreatedAt]);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID(N'[Payments]', N'U') IS NOT NULL DROP TABLE [Payments];");
        }
    }
}