﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TurnTracker.Data;

namespace TurnTracker.Data.Migrations
{
    [DbContext(typeof(TurnContext))]
    partial class TurnContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("TurnTracker.Data.Entities.Activity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedDate");

                    b.Property<DateTimeOffset>("ModifiedDate");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("OwnerId");

                    b.Property<TimeSpan?>("Period");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Activities");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Participant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ActivityId");

                    b.Property<DateTimeOffset>("CreatedDate");

                    b.Property<DateTimeOffset>("ModifiedDate");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("ActivityId");

                    b.HasIndex("UserId");

                    b.ToTable("Participants");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Setting", b =>
                {
                    b.Property<string>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("BoolValue");

                    b.Property<DateTimeOffset>("CreatedDate");

                    b.Property<int>("IntValue");

                    b.Property<DateTimeOffset>("ModifiedDate");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("StringValue");

                    b.Property<string>("Type")
                        .IsRequired();

                    b.HasKey("Key");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Turn", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ActivityId");

                    b.Property<DateTimeOffset>("CreatedDate");

                    b.Property<int>("CreatorId");

                    b.Property<DateTimeOffset>("ModifiedDate");

                    b.Property<DateTimeOffset>("Occurred");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("ActivityId");

                    b.HasIndex("CreatorId");

                    b.HasIndex("UserId");

                    b.ToTable("Turns");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedDate");

                    b.Property<string>("DisplayName")
                        .IsRequired();

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<bool>("EmailVerified");

                    b.Property<byte[]>("Hash")
                        .IsRequired();

                    b.Property<bool>("IsDisabled");

                    b.Property<string>("MobileNumber");

                    b.Property<bool>("MobileNumberVerified");

                    b.Property<DateTimeOffset>("ModifiedDate");

                    b.Property<bool>("MultiFactorEnabled");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("RefreshKey");

                    b.Property<byte>("Role")
                        .HasColumnType("tinyint");

                    b.Property<byte[]>("Salt")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Activity", b =>
                {
                    b.HasOne("TurnTracker.Data.Entities.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Participant", b =>
                {
                    b.HasOne("TurnTracker.Data.Entities.Activity", "Activity")
                        .WithMany("Participants")
                        .HasForeignKey("ActivityId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TurnTracker.Data.Entities.User", "User")
                        .WithMany("Participants")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("TurnTracker.Data.Entities.Turn", b =>
                {
                    b.HasOne("TurnTracker.Data.Entities.Activity", "Activity")
                        .WithMany("Turns")
                        .HasForeignKey("ActivityId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TurnTracker.Data.Entities.User", "Creator")
                        .WithMany("TurnsCreated")
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("TurnTracker.Data.Entities.User", "User")
                        .WithMany("TurnsTaken")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });
#pragma warning restore 612, 618
        }
    }
}
