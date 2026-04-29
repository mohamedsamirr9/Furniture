using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Furniture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBankCodeAndPayoutFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankCode",
                table: "SellerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "SellerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "SellerPayouts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayoutTransactionId",
                table: "SellerPayouts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "SellerPayouts",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankCode",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "SellerPayouts");

            migrationBuilder.DropColumn(
                name: "PayoutTransactionId",
                table: "SellerPayouts");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "SellerPayouts");
        }
    }
}
