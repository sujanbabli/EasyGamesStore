using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyGamesStore.Migrations
{
    /// <inheritdoc />
    public partial class AddProprietorPasswordToShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProprietorPassword",
                table: "Shops",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProprietorPassword",
                table: "Shops");
        }
    }
}
