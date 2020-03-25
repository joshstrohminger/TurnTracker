using Microsoft.EntityFrameworkCore.Migrations;

namespace TurnTracker.Data.Migrations
{
    public partial class AddDeviceToLoginRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logins_DeviceAuthorizations_DeviceAuthorizationId",
                table: "Logins");

            migrationBuilder.AddForeignKey(
                name: "FK_Logins_DeviceAuthorizations_DeviceAuthorizationId",
                table: "Logins",
                column: "DeviceAuthorizationId",
                principalTable: "DeviceAuthorizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logins_DeviceAuthorizations_DeviceAuthorizationId",
                table: "Logins");

            migrationBuilder.AddForeignKey(
                name: "FK_Logins_DeviceAuthorizations_DeviceAuthorizationId",
                table: "Logins",
                column: "DeviceAuthorizationId",
                principalTable: "DeviceAuthorizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
