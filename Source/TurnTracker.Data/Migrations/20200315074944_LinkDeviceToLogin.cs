using Microsoft.EntityFrameworkCore.Migrations;

namespace TurnTracker.Data.Migrations
{
    public partial class LinkDeviceToLogin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeviceAuthorizationId",
                table: "Logins",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Logins_DeviceAuthorizationId",
                table: "Logins",
                column: "DeviceAuthorizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Logins_DeviceAuthorizations_DeviceAuthorizationId",
                table: "Logins",
                column: "DeviceAuthorizationId",
                principalTable: "DeviceAuthorizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logins_DeviceAuthorizations_DeviceAuthorizationId",
                table: "Logins");

            migrationBuilder.DropIndex(
                name: "IX_Logins_DeviceAuthorizationId",
                table: "Logins");

            migrationBuilder.DropColumn(
                name: "DeviceAuthorizationId",
                table: "Logins");
        }
    }
}
