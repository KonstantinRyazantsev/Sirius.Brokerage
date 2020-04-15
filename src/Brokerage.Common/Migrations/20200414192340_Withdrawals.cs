using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brokerage.Common.Migrations
{
    public partial class Withdrawals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "outbox",
                schema: "brokerage",
                columns: table => new
                {
                    RequestId = table.Column<string>(nullable: false),
                    AggregateId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'99999', '1', '', '', 'False', '1'")
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
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    Sequence = table.Column<long>(nullable: false),
                    BrokerAccountId = table.Column<long>(nullable: false),
                    BrokerAccountRequisitesId = table.Column<long>(nullable: false),
                    AccountId = table.Column<long>(nullable: true),
                    ReferenceId = table.Column<string>(nullable: true),
                    AssetId = table.Column<long>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    TenantId = table.Column<string>(nullable: true),
                    DestinationAddress = table.Column<string>(nullable: true),
                    DestinationTag = table.Column<string>(nullable: true),
                    DestinationTagType = table.Column<int>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    TransactionId = table.Column<string>(nullable: true),
                    TransactionBlock = table.Column<long>(nullable: false),
                    TransactionRequiredConfirmationsCount = table.Column<long>(nullable: false),
                    TransactionDateTime = table.Column<DateTimeOffset>(nullable: false),
                    WithdrawalErrorMessage = table.Column<string>(nullable: true),
                    WithdrawalErrorCode = table.Column<int>(nullable: true),
                    WithdrawalOperationId = table.Column<long>(nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_withdrawals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "withdrawals_fees",
                schema: "brokerage",
                columns: table => new
                {
                    WithdrawalId = table.Column<long>(nullable: false),
                    AssetId = table.Column<long>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_withdrawals_fees", x => new { x.WithdrawalId, x.AssetId });
                    table.ForeignKey(
                        name: "FK_withdrawals_fees_withdrawals_WithdrawalId",
                        column: x => x.WithdrawalId,
                        principalSchema: "brokerage",
                        principalTable: "withdrawals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Withdrawal_TransactionId",
                schema: "brokerage",
                table: "withdrawals",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Withdrawal_OperationId",
                schema: "brokerage",
                table: "withdrawals",
                column: "WithdrawalOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_Withdrawal_NaturalId",
                schema: "brokerage",
                table: "withdrawals",
                columns: new[] { "TransactionId", "AssetId", "BrokerAccountRequisitesId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assets",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "outbox",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "withdrawals_fees",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "withdrawals",
                schema: "brokerage");
        }
    }
}
