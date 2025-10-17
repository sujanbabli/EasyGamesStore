using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyGamesStore.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageRecipientsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppMessageReads_AppMessages_AppMessageId1",
                table: "AppMessageReads");

            migrationBuilder.DropIndex(
                name: "IX_AppMessageReads_AppMessageId1",
                table: "AppMessageReads");

            migrationBuilder.DropColumn(
                name: "AppMessageId1",
                table: "AppMessageReads");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppMessageId1",
                table: "AppMessageReads",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppMessageReads_AppMessageId1",
                table: "AppMessageReads",
                column: "AppMessageId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AppMessageReads_AppMessages_AppMessageId1",
                table: "AppMessageReads",
                column: "AppMessageId1",
                principalTable: "AppMessages",
                principalColumn: "Id");
        }
    }
}
