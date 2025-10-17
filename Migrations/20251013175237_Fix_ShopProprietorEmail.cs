using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyGamesStore.Migrations
{
    /// <inheritdoc />
    public partial class Fix_ShopProprietorEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProprietorUserId",
                table: "Shops");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Shops",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Shops",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Shops",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProprietorEmail",
                table: "Shops",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ShopTransfers_ShopId",
                table: "ShopTransfers",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopTransfers_StockItemId",
                table: "ShopTransfers",
                column: "StockItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopTransfers_Shops_ShopId",
                table: "ShopTransfers",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShopTransfers_StockItems_StockItemId",
                table: "ShopTransfers",
                column: "StockItemId",
                principalTable: "StockItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopTransfers_Shops_ShopId",
                table: "ShopTransfers");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopTransfers_StockItems_StockItemId",
                table: "ShopTransfers");

            migrationBuilder.DropIndex(
                name: "IX_ShopTransfers_ShopId",
                table: "ShopTransfers");

            migrationBuilder.DropIndex(
                name: "IX_ShopTransfers_StockItemId",
                table: "ShopTransfers");

            migrationBuilder.DropColumn(
                name: "ProprietorEmail",
                table: "Shops");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Shops",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Shops",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Shops",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "ProprietorUserId",
                table: "Shops",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
