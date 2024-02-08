﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SpinRallyBot;

#nullable disable

namespace SpinRallyBot.Migrations
{
    [DbContext(typeof(SqliteDbContext))]
    [Migration("20240208134247_Fix length of strings")]
    partial class Fixlengthofstrings
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.1");

            modelBuilder.Entity("SpinRallyBot.Models.BackNavigationEntity", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasMaxLength(10000)
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "ChatId");

                    b.ToTable("BackNavigations");
                });

            modelBuilder.Entity("SpinRallyBot.Models.PipelineStateEntity", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasMaxLength(10000)
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "ChatId");

                    b.ToTable("PipelineState");
                });

            modelBuilder.Entity("SpinRallyBot.Models.PlayerEntity", b =>
                {
                    b.Property<string>("PlayerUrl")
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("Fio")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<uint>("Position")
                        .HasColumnType("INTEGER");

                    b.Property<float>("Rating")
                        .HasColumnType("REAL");

                    b.Property<DateTimeOffset>("Updated")
                        .HasColumnType("TEXT");

                    b.HasKey("PlayerUrl");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("SpinRallyBot.Models.SubscriptionEntity", b =>
                {
                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PlayerUrl")
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.HasKey("ChatId", "PlayerUrl");

                    b.HasIndex("PlayerUrl");

                    b.ToTable("Subscriptions");
                });

            modelBuilder.Entity("SpinRallyBot.Models.SubscriptionEntity", b =>
                {
                    b.HasOne("SpinRallyBot.Models.PlayerEntity", "Player")
                        .WithMany("Subscriptions")
                        .HasForeignKey("PlayerUrl")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player");
                });

            modelBuilder.Entity("SpinRallyBot.Models.PlayerEntity", b =>
                {
                    b.Navigation("Subscriptions");
                });
#pragma warning restore 612, 618
        }
    }
}
