using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TurnTracker.Data.Migrations
{
    public partial class AddDefaultNotificationSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Origin",
                table: "NotificationSettings",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DefaultNotificationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(nullable: false),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Sms = table.Column<bool>(nullable: false),
                    Email = table.Column<bool>(nullable: false),
                    Push = table.Column<bool>(nullable: false),
                    ActivityId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultNotificationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefaultNotificationSettings_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefaultNotificationSettings_ActivityId",
                table: "DefaultNotificationSettings",
                column: "ActivityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefaultNotificationSettings");

            migrationBuilder.DropColumn(
                name: "Origin",
                table: "NotificationSettings");
        }
    }
}
