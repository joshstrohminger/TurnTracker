using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TurnTracker.Data.Migrations
{
    public partial class AddCalculatedValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "Users",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "Turns",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "Settings",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasDisabledTurns",
                table: "Participants",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "Participants",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TurnOrder",
                table: "Participants",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TurnsNeeded",
                table: "Participants",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentTurnUserId",
                table: "Activities",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Due",
                table: "Activities",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasDisabledTurns",
                table: "Activities",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TakeTurns",
                table: "Activities",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "Activities",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_CurrentTurnUserId",
                table: "Activities",
                column: "CurrentTurnUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Users_CurrentTurnUserId",
                table: "Activities",
                column: "CurrentTurnUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Users_CurrentTurnUserId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_CurrentTurnUserId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "HasDisabledTurns",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "TurnOrder",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "TurnsNeeded",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "CurrentTurnUserId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Due",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "HasDisabledTurns",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "TakeTurns",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Activities");
        }
    }
}
