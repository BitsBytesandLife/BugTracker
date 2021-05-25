using Microsoft.EntityFrameworkCore.Migrations;

namespace BugTracker.Data.Migrations
{
    public partial class Init_fixed_Schema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_AspNetUsers_RecipientId1",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_AspNetUsers_SenderId1",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketAttachment_AspNetUsers_UserId1",
                table: "TicketAttachment");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketComment_AspNetUsers_UserId1",
                table: "TicketComment");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketHistory_AspNetUsers_UserId1",
                table: "TicketHistory");

            migrationBuilder.DropIndex(
                name: "IX_TicketHistory_UserId1",
                table: "TicketHistory");

            migrationBuilder.DropIndex(
                name: "IX_TicketComment_UserId1",
                table: "TicketComment");

            migrationBuilder.DropIndex(
                name: "IX_TicketAttachment_UserId1",
                table: "TicketAttachment");

            migrationBuilder.DropIndex(
                name: "IX_Notification_RecipientId1",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_SenderId1",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "TicketHistory");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "TicketComment");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "TicketAttachment");

            migrationBuilder.DropColumn(
                name: "RecipientId1",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "SenderId1",
                table: "Notification");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "TicketHistory",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "TicketComment",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "TicketAttachment",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "Notification",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "RecipientId",
                table: "Notification",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_UserId",
                table: "TicketHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComment_UserId",
                table: "TicketComment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachment_UserId",
                table: "TicketAttachment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_RecipientId",
                table: "Notification",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_SenderId",
                table: "Notification",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_AspNetUsers_RecipientId",
                table: "Notification",
                column: "RecipientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_AspNetUsers_SenderId",
                table: "Notification",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAttachment_AspNetUsers_UserId",
                table: "TicketAttachment",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComment_AspNetUsers_UserId",
                table: "TicketComment",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHistory_AspNetUsers_UserId",
                table: "TicketHistory",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_AspNetUsers_RecipientId",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_AspNetUsers_SenderId",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketAttachment_AspNetUsers_UserId",
                table: "TicketAttachment");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketComment_AspNetUsers_UserId",
                table: "TicketComment");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketHistory_AspNetUsers_UserId",
                table: "TicketHistory");

            migrationBuilder.DropIndex(
                name: "IX_TicketHistory_UserId",
                table: "TicketHistory");

            migrationBuilder.DropIndex(
                name: "IX_TicketComment_UserId",
                table: "TicketComment");

            migrationBuilder.DropIndex(
                name: "IX_TicketAttachment_UserId",
                table: "TicketAttachment");

            migrationBuilder.DropIndex(
                name: "IX_Notification_RecipientId",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_SenderId",
                table: "Notification");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "TicketHistory",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "TicketHistory",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "TicketComment",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "TicketComment",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "TicketAttachment",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "TicketAttachment",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SenderId",
                table: "Notification",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "RecipientId",
                table: "Notification",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "RecipientId1",
                table: "Notification",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderId1",
                table: "Notification",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_UserId1",
                table: "TicketHistory",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComment_UserId1",
                table: "TicketComment",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachment_UserId1",
                table: "TicketAttachment",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_RecipientId1",
                table: "Notification",
                column: "RecipientId1");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_SenderId1",
                table: "Notification",
                column: "SenderId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_AspNetUsers_RecipientId1",
                table: "Notification",
                column: "RecipientId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_AspNetUsers_SenderId1",
                table: "Notification",
                column: "SenderId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAttachment_AspNetUsers_UserId1",
                table: "TicketAttachment",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComment_AspNetUsers_UserId1",
                table: "TicketComment",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHistory_AspNetUsers_UserId1",
                table: "TicketHistory",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
