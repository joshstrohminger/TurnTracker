using Microsoft.EntityFrameworkCore.Migrations;

namespace TurnTracker.Data.Migrations
{
    public partial class AllowTurnsToBeDisabled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisablerId",
                table: "Turns",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "Turns",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "PeriodCount",
                table: "Activities",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PeriodUnit",
                table: "Activities",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Turns_DisablerId",
                table: "Turns",
                column: "DisablerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Turns_Users_DisablerId",
                table: "Turns",
                column: "DisablerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turns_Users_DisablerId",
                table: "Turns");

            migrationBuilder.DropIndex(
                name: "IX_Turns_DisablerId",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "DisablerId",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "PeriodCount",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "PeriodUnit",
                table: "Activities");
        }
    }
}
