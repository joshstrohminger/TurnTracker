using Microsoft.EntityFrameworkCore.Migrations;

namespace TurnTracker.Data.Migrations
{
    public partial class RenameTurnColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turns_Users_DisablerId",
                table: "Turns");

            migrationBuilder.RenameColumn(
                name: "DisablerId",
                table: "Turns",
                newName: "ModifierId");

            migrationBuilder.RenameIndex(
                name: "IX_Turns_DisablerId",
                table: "Turns",
                newName: "IX_Turns_ModifierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Turns_Users_ModifierId",
                table: "Turns",
                column: "ModifierId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turns_Users_ModifierId",
                table: "Turns");

            migrationBuilder.RenameColumn(
                name: "ModifierId",
                table: "Turns",
                newName: "DisablerId");

            migrationBuilder.RenameIndex(
                name: "IX_Turns_ModifierId",
                table: "Turns",
                newName: "IX_Turns_DisablerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Turns_Users_DisablerId",
                table: "Turns",
                column: "DisablerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
