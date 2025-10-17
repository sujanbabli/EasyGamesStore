using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyGamesStore.Migrations
{
    /// <inheritdoc />
    public partial class AddProprietorUserIdToShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProprietorUserId",
                table: "Shops",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProprietorUserId",
                table: "Shops");
        }
    }
}
