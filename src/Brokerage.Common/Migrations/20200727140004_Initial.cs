using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brokerage.Common.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "brokerage");

            migrationBuilder.CreateTable(
                name: "assets",
                schema: "brokerage",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BlockchainId = table.Column<string>(nullable: true),
                    Symbol = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Accuracy = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "blockchains",
                schema: "brokerage",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Protocol = table.Column<string>(nullable: true),
                    NetworkType = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blockchains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "broker_account_balances_update",
                schema: "brokerage",
                columns: table => new
                {
                    UpdateId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_broker_account_balances_update", x => x.UpdateId);
                });

            migrationBuilder.CreateTable(
                name: "broker_account_details",
                schema: "brokerage",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10100000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NaturalId = table.Column<string>(nullable: true),
                    ActiveId = table.Column<string>(nullable: true),
                    BlockchainId = table.Column<string>(nullable: true),
                    TenantId = table.Column<string>(nullable: true),
                    BrokerAccountId = table.Column<long>(nullable: false),
                    Address = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_broker_account_details", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "broker_accounts",
                schema: "brokerage",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestId = table.Column<string>(nullable: true),
                    TenantId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    VaultId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_broker_accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "deposits",
                schema: "brokerage",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10500000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    Sequence = table.Column<long>(nullable: false),
                    TenantId = table.Column<string>(nullable: true),
                    BlockchainId = table.Column<string>(nullable: true),
                    BrokerAccountId = table.Column<long>(nullable: false),
                    BrokerAccountDetailsId = table.Column<long>(nullable: false),
                    AccountDetailsId = table.Column<long>(nullable: true),
                    AssetId = table.Column<long>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    ConsolidationOperationId = table.Column<long>(nullable: true),
                    Fees = table.Column<string>(nullable: true),
                    TransactionId = table.Column<string>(nullable: true),
                    TransactionBlock = table.Column<long>(nullable: false),
                    TransactionRequiredConfirmationsCount = table.Column<long>(nullable: false),
                    TransactionDateTime = table.Column<DateTime>(nullable: false),
                    ErrorMessage = table.Column<string>(nullable: true),
                    ErrorCode = table.Column<int>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    Sources = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "detected_transactions",
                schema: "brokerage",
                columns: table => new
                {
                    BlockchainId = table.Column<string>(nullable: false),
                    TransactionId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detected_transactions", x => new { x.BlockchainId, x.TransactionId });
                });

            migrationBuilder.CreateTable(
                name: "operations",
                schema: "brokerage",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActualFees = table.Column<string>(nullable: true),
                    ExpectedFees = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "outbox",
                schema: "brokerage",
                columns: table => new
                {
                    RequestId = table.Column<string>(nullable: false),
                    AggregateId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'2', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Response = table.Column<string>(nullable: true),
                    Events = table.Column<string>(nullable: true),
                    Commands = table.Column<string>(nullable: true),
                    IsStored = table.Column<bool>(nullable: false),
                    IsDispatched = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox", x => x.RequestId);
                });

            migrationBuilder.CreateTable(
                name: "withdrawals",
                schema: "brokerage",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10600000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    Sequence = table.Column<long>(nullable: false),
                    BrokerAccountId = table.Column<long>(nullable: false),
                    BrokerAccountDetailsId = table.Column<long>(nullable: false),
                    AccountId = table.Column<long>(nullable: true),
                    ReferenceId = table.Column<string>(nullable: true),
                    AssetId = table.Column<long>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    TenantId = table.Column<string>(nullable: true),
                    Fees = table.Column<string>(nullable: true),
                    DestinationAddress = table.Column<string>(nullable: true),
                    DestinationTag = table.Column<string>(nullable: true),
                    DestinationTagType = table.Column<int>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    TransactionId = table.Column<string>(nullable: true),
                    TransactionBlock = table.Column<long>(nullable: true),
                    TransactionRequiredConfirmationsCount = table.Column<long>(nullable: true),
                    TransactionDateTime = table.Column<DateTimeOffset>(nullable: true),
                    WithdrawalErrorMessage = table.Column<string>(nullable: true),
                    WithdrawalErrorCode = table.Column<int>(nullable: true),
                    OperationId = table.Column<long>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_withdrawals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                schema: "brokerage",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10200000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrokerAccountId = table.Column<long>(nullable: false),
                    ReferenceId = table.Column<string>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_accounts_broker_accounts_BrokerAccountId",
                        column: x => x.BrokerAccountId,
                        principalSchema: "brokerage",
                        principalTable: "broker_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "broker_account_balances",
                schema: "brokerage",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10400000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NaturalId = table.Column<string>(nullable: true),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    Sequence = table.Column<long>(nullable: false),
                    BrokerAccountId = table.Column<long>(nullable: false),
                    AssetId = table.Column<long>(nullable: false),
                    OwnedBalance = table.Column<decimal>(nullable: false),
                    AvailableBalance = table.Column<decimal>(nullable: false),
                    PendingBalance = table.Column<decimal>(nullable: false),
                    ReservedBalance = table.Column<decimal>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_broker_account_balances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_broker_account_balances_broker_accounts_BrokerAccountId",
                        column: x => x.BrokerAccountId,
                        principalSchema: "brokerage",
                        principalTable: "broker_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "deposit_sources",
                schema: "brokerage",
                columns: table => new
                {
                    DepositId = table.Column<long>(nullable: false),
                    Address = table.Column<string>(nullable: false),
                    DepositEntityId = table.Column<long>(nullable: true),
                    Amount = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposit_sources", x => new { x.DepositId, x.Address });
                    table.ForeignKey(
                        name: "FK_deposit_sources_deposits_DepositEntityId",
                        column: x => x.DepositEntityId,
                        principalSchema: "brokerage",
                        principalTable: "deposits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "withdrawals_fees",
                schema: "brokerage",
                columns: table => new
                {
                    WithdrawalId = table.Column<long>(nullable: false),
                    AssetId = table.Column<long>(nullable: false),
                    WithdrawalEntityId = table.Column<long>(nullable: true),
                    Amount = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_withdrawals_fees", x => new { x.WithdrawalId, x.AssetId });
                    table.ForeignKey(
                        name: "FK_withdrawals_fees_withdrawals_WithdrawalEntityId",
                        column: x => x.WithdrawalEntityId,
                        principalSchema: "brokerage",
                        principalTable: "withdrawals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "account_details",
                schema: "brokerage",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10300000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NaturalId = table.Column<string>(nullable: true),
                    AccountId = table.Column<long>(nullable: false),
                    BrokerAccountId = table.Column<long>(nullable: false),
                    BlockchainId = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Tag = table.Column<string>(nullable: true),
                    TagType = table.Column<int>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_account_details_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "brokerage",
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountDetails_NaturalId",
                schema: "brokerage",
                table: "account_details",
                column: "NaturalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountDetails_AccountId_BlockchainId",
                schema: "brokerage",
                table: "account_details",
                columns: new[] { "AccountId", "BlockchainId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounts_BrokerAccountId",
                schema: "brokerage",
                table: "accounts",
                column: "BrokerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_broker_account_balances_BrokerAccountId",
                schema: "brokerage",
                table: "broker_account_balances",
                column: "BrokerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerAccountBalances_NaturalId",
                schema: "brokerage",
                table: "broker_account_balances",
                column: "NaturalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BrokerAccountDetails_ActiveId",
                schema: "brokerage",
                table: "broker_account_details",
                column: "ActiveId");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerAccountDetails_BrokerAccountId",
                schema: "brokerage",
                table: "broker_account_details",
                column: "BrokerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerAccountDetails_IdDesc",
                schema: "brokerage",
                table: "broker_account_details",
                column: "Id")
                .Annotation("Npgsql:IndexSortOrder", new[] { SortOrder.Descending });

            migrationBuilder.CreateIndex(
                name: "IX_BrokerAccountDetails_NaturalId",
                schema: "brokerage",
                table: "broker_account_details",
                column: "NaturalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BrokerAccount_RequestId",
                schema: "brokerage",
                table: "broker_accounts",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_deposit_sources_DepositEntityId",
                schema: "brokerage",
                table: "deposit_sources",
                column: "DepositEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposit_BlockchainId",
                schema: "brokerage",
                table: "deposits",
                column: "BlockchainId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposit_ConsolidationOperationId",
                schema: "brokerage",
                table: "deposits",
                column: "ConsolidationOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposit_TransactionId",
                schema: "brokerage",
                table: "deposits",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Withdrawal_OperationId",
                schema: "brokerage",
                table: "withdrawals",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_Withdrawal_TransactionId",
                schema: "brokerage",
                table: "withdrawals",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_withdrawals_fees_WithdrawalEntityId",
                schema: "brokerage",
                table: "withdrawals_fees",
                column: "WithdrawalEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_details",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "assets",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "blockchains",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "broker_account_balances",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "broker_account_balances_update",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "broker_account_details",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "deposit_sources",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "detected_transactions",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "operations",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "outbox",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "withdrawals_fees",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "accounts",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "deposits",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "withdrawals",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "broker_accounts",
                schema: "brokerage");
        }
    }
}
