using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TurnTracker.Data.Migrations
{
    public partial class ReworkAuthColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientInfo",
                table: "DeviceAuthorizations");

            migrationBuilder.AddColumn<byte[]>(
                name: "CredentialId",
                table: "DeviceAuthorizations",
                nullable: false,
                defaultValue: new byte[] {  });

            migrationBuilder.AddColumn<byte[]>(
                name: "PublicKey",
                table: "DeviceAuthorizations",
                nullable: false,
                defaultValue: new byte[] {  });

            migrationBuilder.AddColumn<long>(
                name: "SignatureCounter",
                table: "DeviceAuthorizations",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CredentialId",
                table: "DeviceAuthorizations");

            migrationBuilder.DropColumn(
                name: "PublicKey",
                table: "DeviceAuthorizations");

            migrationBuilder.DropColumn(
                name: "SignatureCounter",
                table: "DeviceAuthorizations");

            migrationBuilder.AddColumn<string>(
                name: "ClientInfo",
                table: "DeviceAuthorizations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
