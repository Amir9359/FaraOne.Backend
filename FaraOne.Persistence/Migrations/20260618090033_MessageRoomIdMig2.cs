using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaraOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MessageRoomIdMig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
