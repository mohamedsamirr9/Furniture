using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Furniture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addCommission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlockReason",
                table: "SellerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BlockedAt",
                table: "SellerProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocked",
                table: "SellerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxAllowedCommission",
                table: "SellerProfiles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 10000m);

            migrationBuilder.AddColumn<decimal>(
                name: "PendingCommission",
                table: "SellerProfiles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CommissionTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerProfileId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    OrderTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CommissionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionTransactions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommissionTransactions_SellerProfiles_SellerProfileId",
                        column: x => x.SellerProfileId,
                        principalTable: "SellerProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommissionTransactions_OrderId",
                table: "CommissionTransactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionTransactions_SellerProfileId",
                table: "CommissionTransactions",
                column: "SellerProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommissionTransactions");

            migrationBuilder.DropColumn(
                name: "BlockReason",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "BlockedAt",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "IsBlocked",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "MaxAllowedCommission",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "PendingCommission",
                table: "SellerProfiles");
        }
    }
}
