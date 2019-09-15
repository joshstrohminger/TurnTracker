using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TurnTracker.Data.Migrations
{
    public partial class AddUserProfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Key",
                keyValue: "registration.open");

            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MobileNumberVerified",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MultiFactorEnabled",
                table: "Users",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MobileNumberVerified",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MultiFactorEnabled",
                table: "Users");
        }
    }
}
