using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyGamesStore.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopOrderItems_StockItems_StockItemId",
                table: "ShopOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopStocks_StockItems_StockItemId",
                table: "ShopStocks");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopOrderItems_StockItems_StockItemId",
                table: "ShopOrderItems",
                column: "StockItemId",
                principalTable: "StockItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShopStocks_StockItems_StockItemId",
                table: "ShopStocks",
                column: "StockItemId",
                principalTable: "StockItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopOrderItems_StockItems_StockItemId",
                table: "ShopOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopStocks_StockItems_StockItemId",
                table: "ShopStocks");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopOrderItems_StockItems_StockItemId",
                table: "ShopOrderItems",
                column: "StockItemId",
                principalTable: "StockItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShopStocks_StockItems_StockItemId",
                table: "ShopStocks",
                column: "StockItemId",
                principalTable: "StockItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
