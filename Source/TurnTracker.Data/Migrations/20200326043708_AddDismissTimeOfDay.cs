using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TurnTracker.Data.Migrations
{
    public partial class AddDismissTimeOfDay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "DismissUntilTimeOfDay",
                table: "Participants",
                nullable: false,
                defaultValue: new TimeSpan(0, 18, 0, 0, 0));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DismissUntilTimeOfDay",
                table: "Participants");
        }
    }
}
