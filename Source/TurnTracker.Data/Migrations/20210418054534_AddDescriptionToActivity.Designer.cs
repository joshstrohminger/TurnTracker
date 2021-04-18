﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TurnTracker.Data;

namespace TurnTracker.Data.Migrations
{
    [DbContext(typeof(TurnContext))]
    [Migration("20210418054534_AddDescriptionToActivity")]
    partial class AddDescriptionToActivity
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.3")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("TurnTracker.Data.Entities.Activity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("CurrentTurnUserId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("Due")
                        .HasColumnType("datetimeoffset");

                    b.Property<bool>("HasDisabledTurns")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDisabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("ModifiedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("OwnerId")
                        .HasColumnType("int");

                    b.Property<long?>("Period")
                        .HasColumnType("bigint");

                    b.Property<long?>("PeriodCount")
                        .HasColumnType("bigint");

                    b.Property<string>("PeriodUnit")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TakeTurns")
                        .HasColumnType("bit");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("CurrentTurnUserId");

                    b.HasIndex("OwnerId");

                    b.ToTable("Activities");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.DefaultNotificationSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ActivityId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<bool>("Email")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("ModifiedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<bool>("Push")
                        .HasColumnType("bit");

                    b.Property<bool>("Sms")
                        .HasColumnType("bit");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ActivityId");

                    b.ToTable("DefaultNotificationSettings");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.DeviceAuthorization", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("CredentialId")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("DeviceName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("ModifiedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("PublicKey")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<long>("SignatureCounter")
                        .HasColumnType("bigint");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("DeviceAuthorizations");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Login", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("DeviceAuthorizationId")
                        .HasColumnType("int");

                    b.Property<string>("DeviceName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("ExpirationDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("ModifiedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("RefreshKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DeviceAuthorizationId");

                    b.HasIndex("UserId");

                    b.ToTable("Logins");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.NotificationSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<bool>("Email")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("ModifiedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("NextCheck")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("Origin")
                        .HasColumnType("int");

                    b.Property<int>("ParticipantId")
                        .HasColumnType("int");

                    b.Property<bool>("Push")
                        .HasColumnType("bit");

                    b.Property<bool>("Sms")
                        .HasColumnType("bit");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ParticipantId");

                    b.ToTable("NotificationSettings");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Participant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ActivityId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<TimeSpan>("DismissUntilTimeOfDay")
                        .HasColumnType("time");

                    b.Property<bool>("HasDisabledTurns")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("ModifiedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("TurnOrder")
                        .HasColumnType("int");

                    b.Property<int>("TurnsNeeded")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ActivityId");

                    b.HasIndex("UserId");

                    b.ToTable("Participants");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.PushSubscriptionDevice", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("Endpoint")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Keys")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("ModifiedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("UserId", "Endpoint");

                    b.ToTable("PushSubscriptionDevices");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Setting", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("BoolValue")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("IntValue")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("ModifiedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StringValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Key");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Turn", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ActivityId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("CreatorId")
                        .HasColumnType("int");

                    b.Property<bool>("IsDisabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("ModifiedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("ModifierId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("Occurred")
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ActivityId");

                    b.HasIndex("CreatorId");

                    b.HasIndex("ModifierId");

                    b.HasIndex("UserId");

                    b.ToTable("Turns");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EmailBeingVerified")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("EmailVerificationCreated")
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("EmailVerificationHash")
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("EmailVerificationSalt")
                        .HasColumnType("varbinary(max)");

                    b.Property<bool>("EnablePushNotifications")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDisabled")
                        .HasColumnType("bit");

                    b.Property<string>("MobileNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MobileNumberBeingVerified")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("MobileNumberVerificationCreated")
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("MobileNumberVerificationHash")
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("MobileNumberVerificationSalt")
                        .HasColumnType("varbinary(max)");

                    b.Property<DateTimeOffset>("ModifiedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<bool>("ShowDisabledActivities")
                        .HasColumnType("bit");

                    b.Property<byte>("SnoozeHours")
                        .HasColumnType("tinyint");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Activity", b =>
                {
                    b.HasOne("TurnTracker.Data.Entities.User", "CurrentTurnUser")
                        .WithMany()
                        .HasForeignKey("CurrentTurnUserId");

                    b.HasOne("TurnTracker.Data.Entities.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CurrentTurnUser");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.DefaultNotificationSetting", b =>
                {
                    b.HasOne("TurnTracker.Data.Entities.Activity", "Activity")
                        .WithMany("DefaultNotificationSettings")
                        .HasForeignKey("ActivityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Activity");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.DeviceAuthorization", b =>
                {
                    b.HasOne("TurnTracker.Data.Entities.User", "User")
                        .WithMany("DeviceAuthorizations")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Login", b =>
                {
                    b.HasOne("TurnTracker.Data.Entities.DeviceAuthorization", "DeviceAuthorization")
                        .WithMany("Logins")
                        .HasForeignKey("DeviceAuthorizationId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("TurnTracker.Data.Entities.User", "User")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DeviceAuthorization");

                    b.Navigation("User");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.NotificationSetting", b =>
                {
                    b.HasOne("TurnTracker.Data.Entities.Participant", "Participant")
                        .WithMany("NotificationSettings")
                        .HasForeignKey("ParticipantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Participant");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Participant", b =>
                {
                    b.HasOne("TurnTracker.Data.Entities.Activity", "Activity")
                        .WithMany("Participants")
                        .HasForeignKey("ActivityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TurnTracker.Data.Entities.User", "User")
                        .WithMany("Participants")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Activity");

                    b.Navigation("User");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.PushSubscriptionDevice", b =>
                {
                    b.HasOne("TurnTracker.Data.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Turn", b =>
                {
                    b.HasOne("TurnTracker.Data.Entities.Activity", "Activity")
                        .WithMany("Turns")
                        .HasForeignKey("ActivityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TurnTracker.Data.Entities.User", "Creator")
                        .WithMany("TurnsCreated")
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TurnTracker.Data.Entities.User", "Modifier")
                        .WithMany()
                        .HasForeignKey("ModifierId");

                    b.HasOne("TurnTracker.Data.Entities.User", "User")
                        .WithMany("TurnsTaken")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Activity");

                    b.Navigation("Creator");

                    b.Navigation("Modifier");

                    b.Navigation("User");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Activity", b =>
                {
                    b.Navigation("DefaultNotificationSettings");

                    b.Navigation("Participants");

                    b.Navigation("Turns");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.DeviceAuthorization", b =>
                {
                    b.Navigation("Logins");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Participant", b =>
                {
                    b.Navigation("NotificationSettings");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.User", b =>
                {
                    b.Navigation("DeviceAuthorizations");

                    b.Navigation("Logins");

                    b.Navigation("Participants");

                    b.Navigation("TurnsCreated");

                    b.Navigation("TurnsTaken");
                });
#pragma warning restore 612, 618
        }
    }
}
