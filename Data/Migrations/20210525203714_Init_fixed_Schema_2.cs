using Microsoft.EntityFrameworkCore.Migrations;

namespace BugTracker.Data.Migrations
{
    public partial class Init_fixed_Schema_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_AspNetUsers_DeveloperUserId1",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_AspNetUsers_OwnerUserId1",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_DeveloperUserId1",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_OwnerUserId1",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "DeveloperUserId1",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "OwnerUserId1",
                table: "Ticket");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerUserId",
                table: "Ticket",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "DeveloperUserId",
                table: "Ticket",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_DeveloperUserId",
                table: "Ticket",
                column: "DeveloperUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_OwnerUserId",
                table: "Ticket",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_AspNetUsers_DeveloperUserId",
                table: "Ticket",
                column: "DeveloperUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_AspNetUsers_OwnerUserId",
                table: "Ticket",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_AspNetUsers_DeveloperUserId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_AspNetUsers_OwnerUserId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_DeveloperUserId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_OwnerUserId",
                table: "Ticket");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerUserId",
                table: "Ticket",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DeveloperUserId",
                table: "Ticket",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeveloperUserId1",
                table: "Ticket",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId1",
                table: "Ticket",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_DeveloperUserId1",
                table: "Ticket",
                column: "DeveloperUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_OwnerUserId1",
                table: "Ticket",
                column: "OwnerUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_AspNetUsers_DeveloperUserId1",
                table: "Ticket",
                column: "DeveloperUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_AspNetUsers_OwnerUserId1",
                table: "Ticket",
                column: "OwnerUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
