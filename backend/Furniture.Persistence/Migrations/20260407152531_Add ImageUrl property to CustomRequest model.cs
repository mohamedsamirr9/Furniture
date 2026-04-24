using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Furniture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlpropertytoCustomRequestmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "CustomRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "CustomRequests");
        }
    }
}
