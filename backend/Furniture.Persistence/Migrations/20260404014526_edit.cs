using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Furniture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class edit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomRequest_AspNetUsers_BuyerId",
                table: "CustomRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_Delivery_AspNetUsers_ShipperId",
                table: "Delivery");

            migrationBuilder.DropForeignKey(
                name: "FK_Delivery_Orders_OrderId",
                table: "Delivery");

            migrationBuilder.DropForeignKey(
                name: "FK_Offers_CustomRequest_CustomRequestId",
                table: "Offers");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingBid_AspNetUsers_ShipperId",
                table: "ShippingBid");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingBid_ShippingRequest_ShippingRequestId",
                table: "ShippingBid");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingRequest_Orders_OrderId",
                table: "ShippingRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShippingRequest",
                table: "ShippingRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShippingBid",
                table: "ShippingBid");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Delivery",
                table: "Delivery");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomRequest",
                table: "CustomRequest");

            migrationBuilder.RenameTable(
                name: "ShippingRequest",
                newName: "ShippingRequests");

            migrationBuilder.RenameTable(
                name: "ShippingBid",
                newName: "ShippingBids");

            migrationBuilder.RenameTable(
                name: "Delivery",
                newName: "Deliveries");

            migrationBuilder.RenameTable(
                name: "CustomRequest",
                newName: "CustomRequests");

            migrationBuilder.RenameIndex(
                name: "IX_ShippingRequest_OrderId",
                table: "ShippingRequests",
                newName: "IX_ShippingRequests_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ShippingBid_ShippingRequestId",
                table: "ShippingBids",
                newName: "IX_ShippingBids_ShippingRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_ShippingBid_ShipperId",
                table: "ShippingBids",
                newName: "IX_ShippingBids_ShipperId");

            migrationBuilder.RenameIndex(
                name: "IX_Delivery_ShipperId",
                table: "Deliveries",
                newName: "IX_Deliveries_ShipperId");

            migrationBuilder.RenameIndex(
                name: "IX_Delivery_OrderId",
                table: "Deliveries",
                newName: "IX_Deliveries_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomRequest_BuyerId",
                table: "CustomRequests",
                newName: "IX_CustomRequests_BuyerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShippingRequests",
                table: "ShippingRequests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShippingBids",
                table: "ShippingBids",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Deliveries",
                table: "Deliveries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomRequests",
                table: "CustomRequests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomRequests_AspNetUsers_BuyerId",
                table: "CustomRequests",
                column: "BuyerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_AspNetUsers_ShipperId",
                table: "Deliveries",
                column: "ShipperId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Orders_OrderId",
                table: "Deliveries",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_CustomRequests_CustomRequestId",
                table: "Offers",
                column: "CustomRequestId",
                principalTable: "CustomRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingBids_AspNetUsers_ShipperId",
                table: "ShippingBids",
                column: "ShipperId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingBids_ShippingRequests_ShippingRequestId",
                table: "ShippingBids",
                column: "ShippingRequestId",
                principalTable: "ShippingRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingRequests_Orders_OrderId",
                table: "ShippingRequests",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomRequests_AspNetUsers_BuyerId",
                table: "CustomRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_AspNetUsers_ShipperId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Orders_OrderId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Offers_CustomRequests_CustomRequestId",
                table: "Offers");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingBids_AspNetUsers_ShipperId",
                table: "ShippingBids");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingBids_ShippingRequests_ShippingRequestId",
                table: "ShippingBids");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingRequests_Orders_OrderId",
                table: "ShippingRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShippingRequests",
                table: "ShippingRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShippingBids",
                table: "ShippingBids");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Deliveries",
                table: "Deliveries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomRequests",
                table: "CustomRequests");

            migrationBuilder.RenameTable(
                name: "ShippingRequests",
                newName: "ShippingRequest");

            migrationBuilder.RenameTable(
                name: "ShippingBids",
                newName: "ShippingBid");

            migrationBuilder.RenameTable(
                name: "Deliveries",
                newName: "Delivery");

            migrationBuilder.RenameTable(
                name: "CustomRequests",
                newName: "CustomRequest");

            migrationBuilder.RenameIndex(
                name: "IX_ShippingRequests_OrderId",
                table: "ShippingRequest",
                newName: "IX_ShippingRequest_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ShippingBids_ShippingRequestId",
                table: "ShippingBid",
                newName: "IX_ShippingBid_ShippingRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_ShippingBids_ShipperId",
                table: "ShippingBid",
                newName: "IX_ShippingBid_ShipperId");

            migrationBuilder.RenameIndex(
                name: "IX_Deliveries_ShipperId",
                table: "Delivery",
                newName: "IX_Delivery_ShipperId");

            migrationBuilder.RenameIndex(
                name: "IX_Deliveries_OrderId",
                table: "Delivery",
                newName: "IX_Delivery_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomRequests_BuyerId",
                table: "CustomRequest",
                newName: "IX_CustomRequest_BuyerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShippingRequest",
                table: "ShippingRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShippingBid",
                table: "ShippingBid",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Delivery",
                table: "Delivery",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomRequest",
                table: "CustomRequest",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomRequest_AspNetUsers_BuyerId",
                table: "CustomRequest",
                column: "BuyerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Delivery_AspNetUsers_ShipperId",
                table: "Delivery",
                column: "ShipperId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Delivery_Orders_OrderId",
                table: "Delivery",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_CustomRequest_CustomRequestId",
                table: "Offers",
                column: "CustomRequestId",
                principalTable: "CustomRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingBid_AspNetUsers_ShipperId",
                table: "ShippingBid",
                column: "ShipperId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingBid_ShippingRequest_ShippingRequestId",
                table: "ShippingBid",
                column: "ShippingRequestId",
                principalTable: "ShippingRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingRequest_Orders_OrderId",
                table: "ShippingRequest",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
