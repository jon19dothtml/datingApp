using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class MessageEntityAddedWithAChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Members_RecepientId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_RecepientId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "RecepientId",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RecipientId",
                table: "Messages",
                column: "RecipientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Members_RecipientId",
                table: "Messages",
                column: "RecipientId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Members_RecipientId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_RecipientId",
                table: "Messages");

            migrationBuilder.AddColumn<string>(
                name: "RecepientId",
                table: "Messages",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RecepientId",
                table: "Messages",
                column: "RecepientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Members_RecepientId",
                table: "Messages",
                column: "RecepientId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
