using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Furniture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockReasonAndFixOrderDecimalProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[SellerProfiles]') 
                    AND name = 'BlockReason'
                )
                BEGIN
                    ALTER TABLE [SellerProfiles] ADD [BlockReason] nvarchar(500) NULL;
                END
                ELSE
                BEGIN
                    ALTER TABLE [SellerProfiles] ALTER COLUMN [BlockReason] nvarchar(500) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[SellerProfiles]') 
                    AND name = 'BlockedAt'
                )
                BEGIN
                    ALTER TABLE [SellerProfiles] ADD [BlockedAt] datetime2 NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[SellerProfiles]') 
                    AND name = 'IsBlocked'
                )
                BEGIN
                    ALTER TABLE [SellerProfiles] ADD [IsBlocked] bit NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[SellerProfiles]') 
                    AND name = 'MaxAllowedCommission'
                )
                BEGIN
                    ALTER TABLE [SellerProfiles] ADD [MaxAllowedCommission] decimal(18,2) NOT NULL DEFAULT 10000;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[SellerProfiles]') 
                    AND name = 'PendingCommission'
                )
                BEGIN
                    ALTER TABLE [SellerProfiles] ADD [PendingCommission] decimal(18,2) NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[Orders]') 
                    AND name = 'SubTotal'
                )
                BEGIN
                    ALTER TABLE [Orders] ADD [SubTotal] decimal(18,2) NOT NULL DEFAULT 0;
                END
                ELSE
                BEGIN
                    ALTER TABLE [Orders] ALTER COLUMN [SubTotal] decimal(18,2) NOT NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[Orders]') 
                    AND name = 'ShippingCost'
                )
                BEGIN
                    ALTER TABLE [Orders] ADD [ShippingCost] decimal(18,2) NOT NULL DEFAULT 0;
                END
                ELSE
                BEGIN
                    ALTER TABLE [Orders] ALTER COLUMN [ShippingCost] decimal(18,2) NOT NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[CommissionTransactions]')
                    AND name = 'Id'
                )
                BEGIN
                    CREATE TABLE [CommissionTransactions] (
                        [Id] int NOT NULL IDENTITY,
                        [SellerProfileId] int NOT NULL,
                        [OrderId] int NULL,
                        [CommissionAmount] decimal(18,2) NOT NULL,
                        [OrderTotal] decimal(18,2) NULL,
                        [Type] nvarchar(max) NOT NULL,
                        [Description] nvarchar(max) NULL,
                        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        CONSTRAINT [PK_CommissionTransactions] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_CommissionTransactions_SellerProfiles_SellerProfileId] 
                            FOREIGN KEY ([SellerProfileId]) REFERENCES [SellerProfiles] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_CommissionTransactions_Orders_OrderId] 
                            FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE NO ACTION
                    );
                    CREATE INDEX [IX_CommissionTransactions_OrderId] ON [CommissionTransactions] ([OrderId]);
                    CREATE INDEX [IX_CommissionTransactions_SellerProfileId] ON [CommissionTransactions] ([SellerProfileId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.tables 
                    WHERE name = 'Notifications'
                )
                BEGIN
                    CREATE TABLE [Notifications] (
                        [Id] int NOT NULL IDENTITY,
                        [UserId] nvarchar(450) NOT NULL,
                        [Title] nvarchar(max) NOT NULL,
                        [Message] nvarchar(max) NOT NULL,
                        [IsRead] bit NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [CustomRequestId] int NULL,
                        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Notifications_AspNetUsers_UserId] 
                            FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Notifications");
            migrationBuilder.DropTable(name: "CommissionTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "BlockReason",
                table: "SellerProfiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SubTotal",
                table: "Orders",
                type: "decimal(18,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ShippingCost",
                table: "Orders",
                type: "decimal(18,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}