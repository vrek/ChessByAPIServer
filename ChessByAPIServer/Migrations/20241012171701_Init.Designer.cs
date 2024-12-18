﻿// <auto-generated />
using System;
using ChessByAPIServer;
using ChessByAPIServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ChessByAPIServer.Migrations
{
    [DbContext(typeof(ChessDbContext))]
    [Migration("20241012171701_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ChessByAPIServer.Models.ChessPosition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<Guid>("GameId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsEmpty")
                        .HasColumnType("bit");

                    b.Property<string>("Piece")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Position")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("nvarchar(2)");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("ChessPositions");
                });

            modelBuilder.Entity("ChessByAPIServer.Models.Game", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("BlackPlayerId")
                        .HasColumnType("int");

                    b.Property<int>("WhitePlayerId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BlackPlayerId");

                    b.HasIndex("WhitePlayerId");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("ChessByAPIServer.Models.GameMove", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<Guid>("GameGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("MoveNotation")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<int>("MoveNumber")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GameGuid");

                    b.ToTable("GameMoves");
                });

            modelBuilder.Entity("ChessByAPIServer.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ChessByAPIServer.Models.ChessPosition", b =>
                {
                    b.HasOne("ChessByAPIServer.Models.Game", "Game")
                        .WithMany("ChessPositions")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");
                });

            modelBuilder.Entity("ChessByAPIServer.Models.Game", b =>
                {
                    b.HasOne("ChessByAPIServer.Models.User", "BlackPlayer")
                        .WithMany("BlackGames")
                        .HasForeignKey("BlackPlayerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ChessByAPIServer.Models.User", "WhitePlayer")
                        .WithMany("WhiteGames")
                        .HasForeignKey("WhitePlayerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("BlackPlayer");

                    b.Navigation("WhitePlayer");
                });

            modelBuilder.Entity("ChessByAPIServer.Models.GameMove", b =>
                {
                    b.HasOne("ChessByAPIServer.Models.Game", "Game")
                        .WithMany("GameMoves")
                        .HasForeignKey("GameGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");
                });

            modelBuilder.Entity("ChessByAPIServer.Models.Game", b =>
                {
                    b.Navigation("ChessPositions");

                    b.Navigation("GameMoves");
                });

            modelBuilder.Entity("ChessByAPIServer.Models.User", b =>
                {
                    b.Navigation("BlackGames");

                    b.Navigation("WhiteGames");
                });
#pragma warning restore 612, 618
        }
    }
}
