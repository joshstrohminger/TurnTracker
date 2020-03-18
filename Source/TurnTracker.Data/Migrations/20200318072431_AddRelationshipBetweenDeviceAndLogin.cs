using Microsoft.EntityFrameworkCore.Migrations;

namespace TurnTracker.Data.Migrations
{
    public partial class AddRelationshipBetweenDeviceAndLogin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logins_DeviceAuthorizations_DeviceAuthorizationId",
                table: "Logins");

            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "Logins",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "DeviceAuthorizations",
                nullable: true);

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

            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "Logins");

            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "DeviceAuthorizations");

            migrationBuilder.AddForeignKey(
                name: "FK_Logins_DeviceAuthorizations_DeviceAuthorizationId",
                table: "Logins",
                column: "DeviceAuthorizationId",
                principalTable: "DeviceAuthorizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
