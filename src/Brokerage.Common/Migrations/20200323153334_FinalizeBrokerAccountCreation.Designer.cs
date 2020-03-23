﻿// <auto-generated />
using System;
using Brokerage.Common.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brokerage.Common.Migrations
{
    [DbContext(typeof(BrokerageContext))]
    [Migration("20200323153334_FinalizeBrokerAccountCreation")]
    partial class FinalizeBrokerAccountCreation
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("brokerage")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.BlockchainEntity", b =>
                {
                    b.Property<string>("BlockchainId")
                        .HasColumnType("text");

                    b.HasKey("BlockchainId");

                    b.ToTable("blockchains");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.BrokerAccountEntity", b =>
                {
                    b.Property<long>("BrokerAccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'100000', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime?>("ActivationDateTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime?>("BlockingDateTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("RequestId")
                        .HasColumnType("text");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<string>("TenantId")
                        .HasColumnType("text");

                    b.HasKey("BrokerAccountId");

                    b.HasIndex("RequestId")
                        .IsUnique()
                        .HasName("IX_BrokerAccount_RequestId");

                    b.ToTable("broker_accounts");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.BrokerAccountRequisitesEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'100000', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<string>("BlockchainId")
                        .HasColumnType("text");

                    b.Property<long>("BrokerAccountId")
                        .HasColumnType("bigint");

                    b.Property<string>("RequestId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RequestId")
                        .IsUnique()
                        .HasName("IX_BrokerAccountRequisites_RequestId");

                    b.ToTable("broker_account_requisites");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.ProtocolEntity", b =>
                {
                    b.Property<string>("ProtocolId")
                        .HasColumnType("text");

                    b.HasKey("ProtocolId");

                    b.ToTable("protocols");
                });
#pragma warning restore 612, 618
        }
    }
}
