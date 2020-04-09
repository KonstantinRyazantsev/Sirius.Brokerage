﻿// <auto-generated />
using System;
using Brokerage.Common.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brokerage.Common.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("brokerage")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.AccountEntity", b =>
                {
                    b.Property<long>("AccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'100000', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTimeOffset?>("ActivationDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("BlockingDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("BrokerAccountId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("CreationDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ReferenceId")
                        .HasColumnType("text");

                    b.Property<string>("RequestId")
                        .HasColumnType("text");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.HasKey("AccountId");

                    b.HasIndex("BrokerAccountId");

                    b.HasIndex("RequestId")
                        .IsUnique()
                        .HasName("IX_Account_RequestId");

                    b.ToTable("accounts");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.AccountRequisitesEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'100000', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("AccountId")
                        .HasColumnType("bigint");

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<string>("BlockchainId")
                        .HasColumnType("text");

                    b.Property<long>("BrokerAccountId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("CreationDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RequestId")
                        .HasColumnType("text");

                    b.Property<string>("Tag")
                        .HasColumnType("text");

                    b.Property<int?>("TagType")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("RequestId")
                        .IsUnique()
                        .HasName("IX_AccountRequisites_RequestId");

                    b.HasIndex("BlockchainId", "Address")
                        .HasName("IX_AccountRequisites_BlockchainId_Address");

                    b.ToTable("account_requisites");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.BrokerAccountBalancesEntity", b =>
                {
                    b.Property<long>("BrokerAccountBalancesId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("AssetId")
                        .HasColumnType("bigint");

                    b.Property<decimal>("AvailableBalance")
                        .HasColumnType("numeric");

                    b.Property<DateTimeOffset>("AvailableBalanceUpdateDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("BrokerAccountId")
                        .HasColumnType("bigint");

                    b.Property<decimal>("OwnedBalance")
                        .HasColumnType("numeric");

                    b.Property<DateTimeOffset>("OwnedBalanceUpdateDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("PendingBalance")
                        .HasColumnType("numeric");

                    b.Property<DateTimeOffset>("PendingBalanceUpdateDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("ReservedBalance")
                        .HasColumnType("numeric");

                    b.Property<DateTimeOffset>("ReservedBalanceUpdateDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("Sequence")
                        .HasColumnType("bigint");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("xmin")
                        .HasColumnType("xid");

                    b.HasKey("BrokerAccountBalancesId");

                    b.HasIndex("BrokerAccountId", "AssetId")
                        .HasName("IX_BrokerAccountBalances_BrokerAccountId_AssetId");

                    b.ToTable("broker_account_balances");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.BrokerAccountBalancesUpdateEntity", b =>
                {
                    b.Property<string>("UpdateId")
                        .HasColumnType("text");

                    b.HasKey("UpdateId");

                    b.ToTable("broker_account_balances_update");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.BrokerAccountEntity", b =>
                {
                    b.Property<long>("BrokerAccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'100000', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTimeOffset?>("ActivationDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("BlockingDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("CreationDateTime")
                        .HasColumnType("timestamp with time zone");

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

                    b.Property<DateTimeOffset>("CreationDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RequestId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("BlockchainId")
                        .HasName("IX_BrokerAccountRequisites_BlockchainId");

                    b.HasIndex("BrokerAccountId")
                        .HasName("IX_BrokerAccountRequisites_BrokerAccountId");

                    b.HasIndex("RequestId")
                        .IsUnique()
                        .HasName("IX_BrokerAccountRequisites_RequestId");

                    b.ToTable("broker_account_requisites");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.Deposits.DepositEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'100000', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long?>("AccountRequisitesId")
                        .HasColumnType("bigint");

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<long>("AssetId")
                        .HasColumnType("bigint");

                    b.Property<long>("BrokerAccountRequisitesId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset?>("CancelledDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("CompletedDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("ConfirmedDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("DepositState")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("DetectedDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("ErrorCode")
                        .HasColumnType("integer");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("FailedDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long?>("OperationId")
                        .HasColumnType("bigint");

                    b.Property<long>("Sequence")
                        .HasColumnType("bigint");

                    b.Property<long>("TransactionBlock")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("TransactionDateTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("TransactionId")
                        .HasColumnType("text");

                    b.Property<long>("TransactionRequiredConfirmationsCount")
                        .HasColumnType("bigint");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("xmin")
                        .HasColumnType("xid");

                    b.HasKey("Id");

                    b.HasIndex("OperationId")
                        .HasName("IX_Deposit_OperationId");

                    b.HasIndex("TransactionId")
                        .HasName("IX_Deposit_TransactionId");

                    b.HasIndex("TransactionId", "AssetId", "BrokerAccountRequisitesId", "AccountRequisitesId")
                        .IsUnique()
                        .HasName("IX_Deposit_NaturalId");

                    b.ToTable("deposits");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.Deposits.DepositFeeEntity", b =>
                {
                    b.Property<long>("DepositId")
                        .HasColumnType("bigint");

                    b.Property<long>("AssetId")
                        .HasColumnType("bigint");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.HasKey("DepositId", "AssetId");

                    b.ToTable("deposit_fees");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.Deposits.DepositSourceEntity", b =>
                {
                    b.Property<long>("DepositId")
                        .HasColumnType("bigint");

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.HasKey("DepositId", "Address");

                    b.ToTable("deposit_sources");
                });

            modelBuilder.Entity("Brokerage.Common.ReadModels.Blockchains.Blockchain", b =>
                {
                    b.Property<string>("BlockchainId")
                        .HasColumnType("text");

                    b.Property<string>("IntegrationUrl")
                        .HasColumnType("text");

                    b.HasKey("BlockchainId");

                    b.ToTable("blockchains");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.AccountEntity", b =>
                {
                    b.HasOne("Brokerage.Common.Persistence.Entities.BrokerAccountEntity", "BrokerAccount")
                        .WithMany("Accounts")
                        .HasForeignKey("BrokerAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.AccountRequisitesEntity", b =>
                {
                    b.HasOne("Brokerage.Common.Persistence.Entities.AccountEntity", "Account")
                        .WithMany("AccountRequisites")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.BrokerAccountBalancesEntity", b =>
                {
                    b.HasOne("Brokerage.Common.Persistence.Entities.BrokerAccountEntity", "BrokerAccount")
                        .WithMany()
                        .HasForeignKey("BrokerAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.Deposits.DepositFeeEntity", b =>
                {
                    b.HasOne("Brokerage.Common.Persistence.Entities.Deposits.DepositEntity", "DepositEntity")
                        .WithMany("Fees")
                        .HasForeignKey("DepositId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Entities.Deposits.DepositSourceEntity", b =>
                {
                    b.HasOne("Brokerage.Common.Persistence.Entities.Deposits.DepositEntity", "DepositEntity")
                        .WithMany("Sources")
                        .HasForeignKey("DepositId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
