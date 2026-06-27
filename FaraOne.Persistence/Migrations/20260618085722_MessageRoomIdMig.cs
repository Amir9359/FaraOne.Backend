using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaraOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MessageRoomIdMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChatRooms_ChatRoomId",
                schema: "dbo",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_RoomId",
                schema: "dbo",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "RoomId",
                schema: "dbo",
                table: "Messages");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_ChatRoomId",
                schema: "dbo",
                table: "Messages",
                newName: "IX_Messages_RoomId");

            migrationBuilder.AlterColumn<string>(
                name: "ChatRoomId",
                schema: "dbo",
                table: "Messages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ChatRoomId1",
                schema: "dbo",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatRoomId1",
                schema: "dbo",
                table: "Messages",
                column: "ChatRoomId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ChatRooms_ChatRoomId1",
                schema: "dbo",
                table: "Messages",
                column: "ChatRoomId1",
                principalSchema: "dbo",
                principalTable: "ChatRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChatRooms_ChatRoomId1",
                schema: "dbo",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChatRoomId1",
                schema: "dbo",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ChatRoomId1",
                schema: "dbo",
                table: "Messages");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_RoomId",
                schema: "dbo",
                table: "Messages",
                newName: "IX_Messages_ChatRoomId");

            migrationBuilder.AlterColumn<int>(
                name: "ChatRoomId",
                schema: "dbo",
                table: "Messages",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "RoomId",
                schema: "dbo",
                table: "Messages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RoomId",
                schema: "dbo",
                table: "Messages",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ChatRooms_ChatRoomId",
                schema: "dbo",
                table: "Messages",
                column: "ChatRoomId",
                principalSchema: "dbo",
                principalTable: "ChatRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
