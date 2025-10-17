using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyGamesStore.Migrations
{
    /// <inheritdoc />
    public partial class AddShopOrderIdToUserSalesHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSalesHistories_Orders_OrderId",
                table: "UserSalesHistories");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "UserSalesHistories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ShopOrderId",
                table: "UserSalesHistories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSalesHistories_ShopOrderId",
                table: "UserSalesHistories",
                column: "ShopOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSalesHistories_Orders_OrderId",
                table: "UserSalesHistories",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSalesHistories_ShopOrders_ShopOrderId",
                table: "UserSalesHistories",
                column: "ShopOrderId",
                principalTable: "ShopOrders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSalesHistories_Orders_OrderId",
                table: "UserSalesHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSalesHistories_ShopOrders_ShopOrderId",
                table: "UserSalesHistories");

            migrationBuilder.DropIndex(
                name: "IX_UserSalesHistories_ShopOrderId",
                table: "UserSalesHistories");

            migrationBuilder.DropColumn(
                name: "ShopOrderId",
                table: "UserSalesHistories");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "UserSalesHistories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSalesHistories_Orders_OrderId",
                table: "UserSalesHistories",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
