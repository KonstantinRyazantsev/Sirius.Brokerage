﻿// <auto-generated />
using System;
using Brokerage.Bilv1.Repositories.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brokerage.Bilv1.Repositories.Migrations
{
    [DbContext(typeof(BrokerageBilV1Context))]
    partial class BrokerageBilV1ContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("brokerage_bil_v1")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Brokerage.Bilv1.Repositories.Entities.EnrolledBalanceEntity", b =>
                {
                    b.Property<string>("BlockchianId")
                        .HasColumnType("text");

                    b.Property<string>("BlockchainAssetId")
                        .HasColumnType("text");

                    b.Property<string>("WalletAddress")
                        .HasColumnType("text");

                    b.Property<decimal>("Balance")
                        .HasColumnType("numeric");

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<string>("OriginalWalletAddress")
                        .HasColumnType("text");

                    b.HasKey("BlockchianId", "BlockchainAssetId", "WalletAddress");

                    b.ToTable("enrolled_balance");
                });

            modelBuilder.Entity("Brokerage.Bilv1.Repositories.Entities.OperationEntity", b =>
                {
                    b.Property<string>("BlockchianId")
                        .HasColumnType("text");

                    b.Property<string>("BlockchainAssetId")
                        .HasColumnType("text");

                    b.Property<string>("WalletAddress")
                        .HasColumnType("text");

                    b.Property<long>("OperationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("BalanceChange")
                        .HasColumnType("numeric");

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<string>("OriginalWalletAddress")
                        .HasColumnType("text");

                    b.HasKey("BlockchianId", "BlockchainAssetId", "WalletAddress", "OperationId");

                    b.ToTable("operations");
                });

            modelBuilder.Entity("Brokerage.Bilv1.Repositories.Entities.WalletEntity", b =>
                {
                    b.Property<string>("BlockchainId")
                        .HasColumnType("text");

                    b.Property<string>("NetworkId")
                        .HasColumnType("text");

                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("ImportedDateTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsCompromised")
                        .HasColumnType("boolean");

                    b.Property<string>("OriginalWalletAddress")
                        .HasColumnType("text");

                    b.Property<string>("PublicKey")
                        .HasColumnType("text");

                    b.Property<string>("WalletAddress")
                        .HasColumnType("text");

                    b.HasKey("BlockchainId", "NetworkId", "Id");

                    b.HasIndex("BlockchainId", "NetworkId", "WalletAddress")
                        .IsUnique()
                        .HasName("IX_BlockchainId_NetworkId_WalletAddress");

                    b.ToTable("wallets");
                });
#pragma warning restore 612, 618
        }
    }
}
