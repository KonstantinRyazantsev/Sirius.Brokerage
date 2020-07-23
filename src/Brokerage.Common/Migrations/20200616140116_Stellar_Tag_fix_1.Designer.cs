﻿// <auto-generated />
using System;
using Brokerage.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brokerage.Common.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20200616140116_Stellar_Tag_fix_1")]
    partial class Stellar_Tag_fix_1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("brokerage")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Brokerage.Common.Persistence.Accounts.AccountDetailsEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'10300000', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("AccountId")
                        .HasColumnType("bigint");

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<string>("BlockchainId")
                        .HasColumnType("text");

                    b.Property<long>("BrokerAccountId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NaturalId")
                        .HasColumnType("text");

                    b.Property<string>("Tag")
                        .HasColumnType("text");

                    b.Property<int?>("TagType")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("NaturalId")
                        .IsUnique()
                        .HasName("IX_AccountDetails_NaturalId");

                    b.HasIndex("AccountId", "BlockchainId")
                        .IsUnique()
                        .HasName("IX_AccountDetails_AccountId_BlockchainId");

                    b.ToTable("account_details");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Accounts.AccountEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'10200000', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("BrokerAccountId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ReferenceId")
                        .HasColumnType("text");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("BrokerAccountId");

                    b.ToTable("accounts");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.BrokerAccount.BrokerAccountBalancesEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'10400000', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("AssetId")
                        .HasColumnType("bigint");

                    b.Property<decimal>("AvailableBalance")
                        .HasColumnType("numeric");

                    b.Property<long>("BrokerAccountId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NaturalId")
                        .HasColumnType("text");

                    b.Property<decimal>("OwnedBalance")
                        .HasColumnType("numeric");

                    b.Property<decimal>("PendingBalance")
                        .HasColumnType("numeric");

                    b.Property<decimal>("ReservedBalance")
                        .HasColumnType("numeric");

                    b.Property<long>("Sequence")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("xmin")
                        .HasColumnType("xid");

                    b.HasKey("Id");

                    b.HasIndex("BrokerAccountId");

                    b.HasIndex("NaturalId")
                        .IsUnique()
                        .HasName("IX_BrokerAccountBalances_NaturalId");

                    b.ToTable("broker_account_balances");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.BrokerAccount.BrokerAccountBalancesUpdateEntity", b =>
                {
                    b.Property<string>("UpdateId")
                        .HasColumnType("text");

                    b.HasKey("UpdateId");

                    b.ToTable("broker_account_balances_update");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.BrokerAccount.BrokerAccountDetailsEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'10100000', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ActiveId")
                        .HasColumnType("text");

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<string>("BlockchainId")
                        .HasColumnType("text");

                    b.Property<long>("BrokerAccountId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NaturalId")
                        .HasColumnType("text");

                    b.Property<string>("TenantId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ActiveId")
                        .HasName("IX_BrokerAccountDetails_ActiveId");

                    b.HasIndex("BrokerAccountId")
                        .HasName("IX_BrokerAccountDetails_BrokerAccountId");

                    b.HasIndex("Id")
                        .HasName("IX_BrokerAccountDetails_IdDesc")
                        .HasAnnotation("Npgsql:IndexSortOrder", new[] { SortOrder.Descending });

                    b.HasIndex("NaturalId")
                        .IsUnique()
                        .HasName("IX_BrokerAccountDetails_NaturalId");

                    b.ToTable("broker_account_details");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.BrokerAccount.BrokerAccountEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'10000000', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("RequestId")
                        .HasColumnType("text");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<string>("TenantId")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("VaultId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("RequestId")
                        .IsUnique()
                        .HasName("IX_BrokerAccount_RequestId");

                    b.ToTable("broker_accounts");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Deposits.DepositEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'10500000', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long?>("AccountDetailsId")
                        .HasColumnType("bigint");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<long>("AssetId")
                        .HasColumnType("bigint");

                    b.Property<string>("BlockchainId")
                        .HasColumnType("text");

                    b.Property<long>("BrokerAccountDetailsId")
                        .HasColumnType("bigint");

                    b.Property<long>("BrokerAccountId")
                        .HasColumnType("bigint");

                    b.Property<long?>("ConsolidationOperationId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("ErrorCode")
                        .HasColumnType("integer");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<long>("Sequence")
                        .HasColumnType("bigint");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<string>("TenantId")
                        .HasColumnType("text");

                    b.Property<long>("TransactionBlock")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("TransactionDateTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("TransactionId")
                        .HasColumnType("text");

                    b.Property<long>("TransactionRequiredConfirmationsCount")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("xmin")
                        .HasColumnType("xid");

                    b.HasKey("Id");

                    b.HasIndex("ConsolidationOperationId")
                        .IsUnique()
                        .HasName("IX_Deposit_ConsolidationOperationId");

                    b.HasIndex("BlockchainId", "TransactionId")
                        .HasName("IX_Deposit_BlockchainId_TransactionId");

                    b.ToTable("deposits");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Deposits.DepositFeeEntity", b =>
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

            modelBuilder.Entity("Brokerage.Common.Persistence.Deposits.DepositSourceEntity", b =>
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

            modelBuilder.Entity("Brokerage.Common.Persistence.Operations.OperationEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("operations");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Transactions.DetectedTransactionEntity", b =>
                {
                    b.Property<string>("BlockchainId")
                        .HasColumnType("text");

                    b.Property<string>("TransactionId")
                        .HasColumnType("text");

                    b.HasKey("BlockchainId", "TransactionId");

                    b.ToTable("detected_transactions");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Withdrawals.WithdrawalEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'10600000', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long?>("AccountId")
                        .HasColumnType("bigint");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<long>("AssetId")
                        .HasColumnType("bigint");

                    b.Property<long>("BrokerAccountDetailsId")
                        .HasColumnType("bigint");

                    b.Property<long>("BrokerAccountId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DestinationAddress")
                        .HasColumnType("text");

                    b.Property<string>("DestinationTag")
                        .HasColumnType("text");

                    b.Property<int?>("DestinationTagType")
                        .HasColumnType("integer");

                    b.Property<long?>("OperationId")
                        .HasColumnType("bigint");

                    b.Property<string>("ReferenceId")
                        .HasColumnType("text");

                    b.Property<long>("Sequence")
                        .HasColumnType("bigint");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<string>("TenantId")
                        .HasColumnType("text");

                    b.Property<long?>("TransactionBlock")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset?>("TransactionDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("TransactionId")
                        .HasColumnType("text");

                    b.Property<long?>("TransactionRequiredConfirmationsCount")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("xmin")
                        .HasColumnType("xid");

                    b.Property<int?>("WithdrawalErrorCode")
                        .HasColumnType("integer");

                    b.Property<string>("WithdrawalErrorMessage")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("OperationId")
                        .HasName("IX_Withdrawal_OperationId");

                    b.HasIndex("TransactionId")
                        .HasName("IX_Withdrawal_TransactionId");

                    b.ToTable("withdrawals");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Withdrawals.WithdrawalFeeEntity", b =>
                {
                    b.Property<long>("WithdrawalId")
                        .HasColumnType("bigint");

                    b.Property<long>("AssetId")
                        .HasColumnType("bigint");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.HasKey("WithdrawalId", "AssetId");

                    b.ToTable("withdrawals_fees");
                });

            modelBuilder.Entity("Brokerage.Common.ReadModels.Assets.Asset", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("Accuracy")
                        .HasColumnType("integer");

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<string>("BlockchainId")
                        .HasColumnType("text");

                    b.Property<string>("Symbol")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("assets");
                });

            modelBuilder.Entity("Brokerage.Common.ReadModels.Blockchains.Blockchain", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("xmin")
                        .HasColumnType("xid");

                    b.HasKey("Id");

                    b.ToTable("blockchains");
                });

            modelBuilder.Entity("Swisschain.Extensions.Idempotency.EfCore.OutboxEntity", b =>
                {
                    b.Property<string>("RequestId")
                        .HasColumnType("text");

                    b.Property<long>("AggregateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'2', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Commands")
                        .HasColumnType("text");

                    b.Property<string>("Events")
                        .HasColumnType("text");

                    b.Property<bool>("IsDispatched")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsStored")
                        .HasColumnType("boolean");

                    b.Property<string>("Response")
                        .HasColumnType("text");

                    b.HasKey("RequestId");

                    b.ToTable("outbox");
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Accounts.AccountDetailsEntity", b =>
                {
                    b.HasOne("Brokerage.Common.Persistence.Accounts.AccountEntity", "Account")
                        .WithMany("AccountDetails")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Accounts.AccountEntity", b =>
                {
                    b.HasOne("Brokerage.Common.Persistence.BrokerAccount.BrokerAccountEntity", "BrokerAccount")
                        .WithMany("Accounts")
                        .HasForeignKey("BrokerAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.BrokerAccount.BrokerAccountBalancesEntity", b =>
                {
                    b.HasOne("Brokerage.Common.Persistence.BrokerAccount.BrokerAccountEntity", "BrokerAccount")
                        .WithMany()
                        .HasForeignKey("BrokerAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Deposits.DepositFeeEntity", b =>
                {
                    b.HasOne("Brokerage.Common.Persistence.Deposits.DepositEntity", "DepositEntity")
                        .WithMany("Fees")
                        .HasForeignKey("DepositId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Deposits.DepositSourceEntity", b =>
                {
                    b.HasOne("Brokerage.Common.Persistence.Deposits.DepositEntity", "DepositEntity")
                        .WithMany("Sources")
                        .HasForeignKey("DepositId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Brokerage.Common.Persistence.Withdrawals.WithdrawalFeeEntity", b =>
                {
                    b.HasOne("Brokerage.Common.Persistence.Withdrawals.WithdrawalEntity", "WithdrawalEntity")
                        .WithMany("Fees")
                        .HasForeignKey("WithdrawalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Brokerage.Common.ReadModels.Blockchains.Blockchain", b =>
                {
                    b.OwnsOne("Brokerage.Common.ReadModels.Blockchains.Protocol", "Protocol", b1 =>
                        {
                            b1.Property<string>("BlockchainId")
                                .HasColumnType("text");

                            b1.Property<string>("Code")
                                .HasColumnName("ProtocolCode")
                                .HasColumnType("text");

                            b1.HasKey("BlockchainId");

                            b1.HasIndex("Code")
                                .HasName("IX_Blockchain_ProtocolCode");

                            b1.ToTable("blockchains");

                            b1.WithOwner()
                                .HasForeignKey("BlockchainId");

                            b1.OwnsOne("Brokerage.Common.ReadModels.Blockchains.Capabilities", "Capabilities", b2 =>
                                {
                                    b2.Property<string>("ProtocolBlockchainId")
                                        .HasColumnType("text");

                                    b2.HasKey("ProtocolBlockchainId");

                                    b2.ToTable("blockchains");

                                    b2.WithOwner()
                                        .HasForeignKey("ProtocolBlockchainId");

                                    b2.OwnsOne("Brokerage.Common.ReadModels.Blockchains.DestinationTagCapabilities", "DestinationTag", b3 =>
                                        {
                                            b3.Property<string>("CapabilitiesProtocolBlockchainId")
                                                .HasColumnType("text");

                                            b3.HasKey("CapabilitiesProtocolBlockchainId");

                                            b3.ToTable("blockchains");

                                            b3.WithOwner()
                                                .HasForeignKey("CapabilitiesProtocolBlockchainId");

                                            b3.OwnsOne("Brokerage.Common.ReadModels.Blockchains.NumberDestinationTagsCapabilities", "Number", b4 =>
                                                {
                                                    b4.Property<string>("DestinationTagCapabilitiesCapabilitiesProtocolBlockchainId")
                                                        .HasColumnType("text");

                                                    b4.Property<long>("Max")
                                                        .HasColumnType("bigint");

                                                    b4.Property<long>("Min")
                                                        .HasColumnType("bigint");

                                                    b4.HasKey("DestinationTagCapabilitiesCapabilitiesProtocolBlockchainId");

                                                    b4.ToTable("blockchains");

                                                    b4.WithOwner()
                                                        .HasForeignKey("DestinationTagCapabilitiesCapabilitiesProtocolBlockchainId");
                                                });

                                            b3.OwnsOne("Brokerage.Common.ReadModels.Blockchains.TextDestinationTagsCapabilities", "Text", b4 =>
                                                {
                                                    b4.Property<string>("DestinationTagCapabilitiesCapabilitiesProtocolBlockchainId")
                                                        .HasColumnType("text");

                                                    b4.Property<long>("MaxLength")
                                                        .HasColumnType("bigint");

                                                    b4.HasKey("DestinationTagCapabilitiesCapabilitiesProtocolBlockchainId");

                                                    b4.ToTable("blockchains");

                                                    b4.WithOwner()
                                                        .HasForeignKey("DestinationTagCapabilitiesCapabilitiesProtocolBlockchainId");
                                                });
                                        });
                                });
                        });
                });
#pragma warning restore 612, 618
        }
    }
}