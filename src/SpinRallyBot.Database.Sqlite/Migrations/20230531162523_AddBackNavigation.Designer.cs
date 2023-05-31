﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SpinRallyBot;

#nullable disable

namespace SpinRallyBot.Migrations
{
    [DbContext(typeof(SqliteDbContext))]
    [Migration("20230531162523_AddBackNavigation")]
    partial class AddBackNavigation
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("SpinRallyBot.Models.BackNavigation", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "ChatId");

                    b.ToTable("BackNavigations");
                });

            modelBuilder.Entity("SpinRallyBot.Models.PipelineState", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "ChatId");

                    b.ToTable("PipelineState");
                });

            modelBuilder.Entity("SpinRallyBot.Models.Subscription", b =>
                {
                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PlayerUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Fio")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ChatId", "PlayerUrl");

                    b.ToTable("Subscriptions");
                });
#pragma warning restore 612, 618
        }
    }
}
