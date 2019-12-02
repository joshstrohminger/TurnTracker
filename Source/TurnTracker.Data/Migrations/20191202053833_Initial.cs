using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TurnTracker.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Key = table.Column<string>(nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(nullable: false),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    IntValue = table.Column<int>(nullable: false),
                    StringValue = table.Column<string>(nullable: true),
                    BoolValue = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(nullable: false),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Username = table.Column<string>(nullable: false),
                    DisplayName = table.Column<string>(nullable: false),
                    IsDisabled = table.Column<bool>(nullable: false),
                    Role = table.Column<int>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    EmailVerificationHash = table.Column<byte[]>(nullable: true),
                    EmailVerificationSalt = table.Column<byte[]>(nullable: true),
                    EmailVerificationCreated = table.Column<DateTimeOffset>(nullable: true),
                    EmailBeingVerified = table.Column<string>(nullable: true),
                    MobileNumber = table.Column<string>(nullable: true),
                    MobileNumberVerificationHash = table.Column<byte[]>(nullable: true),
                    MobileNumberVerificationSalt = table.Column<byte[]>(nullable: true),
                    MobileNumberVerificationCreated = table.Column<DateTimeOffset>(nullable: true),
                    MobileNumberBeingVerified = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<byte[]>(nullable: false),
                    PasswordSalt = table.Column<byte[]>(nullable: false),
                    RefreshKey = table.Column<string>(nullable: true),
                    ShowDisabledActivities = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(nullable: false),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: true),
                    IsDisabled = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Period = table.Column<long>(nullable: true),
                    PeriodUnit = table.Column<string>(nullable: true),
                    PeriodCount = table.Column<long>(nullable: true),
                    TakeTurns = table.Column<bool>(nullable: false),
                    OwnerId = table.Column<int>(nullable: false),
                    CurrentTurnUserId = table.Column<int>(nullable: true),
                    Due = table.Column<DateTimeOffset>(nullable: true),
                    HasDisabledTurns = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activities_Users_CurrentTurnUserId",
                        column: x => x.CurrentTurnUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Activities_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(nullable: false),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: true),
                    UserId = table.Column<int>(nullable: false),
                    ActivityId = table.Column<int>(nullable: false),
                    TurnsNeeded = table.Column<int>(nullable: false),
                    TurnOrder = table.Column<int>(nullable: false),
                    HasDisabledTurns = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participants_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Turns",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(nullable: false),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: true),
                    UserId = table.Column<int>(nullable: false),
                    Occurred = table.Column<DateTimeOffset>(nullable: false),
                    CreatorId = table.Column<int>(nullable: false),
                    ModifierId = table.Column<int>(nullable: true),
                    ActivityId = table.Column<int>(nullable: false),
                    IsDisabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turns_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Turns_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Turns_Users_ModifierId",
                        column: x => x.ModifierId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Turns_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationSettings",
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
                    ParticipantId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationSettings_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_CurrentTurnUserId",
                table: "Activities",
                column: "CurrentTurnUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_OwnerId",
                table: "Activities",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSettings_ParticipantId",
                table: "NotificationSettings",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_ActivityId",
                table: "Participants",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_UserId",
                table: "Participants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_ActivityId",
                table: "Turns",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_CreatorId",
                table: "Turns",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_ModifierId",
                table: "Turns",
                column: "ModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_UserId",
                table: "Turns",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationSettings");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Turns");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
