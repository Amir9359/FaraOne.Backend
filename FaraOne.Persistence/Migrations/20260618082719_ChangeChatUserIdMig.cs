using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaraOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeChatUserIdMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_Users_UserId1",
                schema: "dbo",
                table: "ChatRooms");

            migrationBuilder.DropIndex(
                name: "IX_ChatRooms_UserId1",
                schema: "dbo",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "UserId1",
                schema: "dbo",
                table: "ChatRooms");

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                schema: "dbo",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "dbo",
                table: "ChatRooms",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_UserId",
                schema: "dbo",
                table: "ChatRooms",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_Users_UserId",
                schema: "dbo",
                table: "ChatRooms",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_Users_UserId",
                schema: "dbo",
                table: "ChatRooms");

            migrationBuilder.DropIndex(
                name: "IX_ChatRooms_UserId",
                schema: "dbo",
                table: "ChatRooms");

            migrationBuilder.AlterColumn<int>(
                name: "SenderId",
                schema: "dbo",
                table: "Messages",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                schema: "dbo",
                table: "ChatRooms",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                schema: "dbo",
                table: "ChatRooms",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_UserId1",
                schema: "dbo",
                table: "ChatRooms",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_Users_UserId1",
                schema: "dbo",
                table: "ChatRooms",
                column: "UserId1",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
