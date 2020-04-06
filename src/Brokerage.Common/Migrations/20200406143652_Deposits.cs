using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brokerage.Common.Migrations
{
    public partial class Deposits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deposits",
                schema: "brokerage",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    Sequence = table.Column<long>(nullable: false),
                    BrokerAccountRequisitesId = table.Column<long>(nullable: false),
                    AccountRequisitesId = table.Column<long>(nullable: true),
                    AssetId = table.Column<long>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    TransactionId = table.Column<string>(nullable: true),
                    TransactionBlock = table.Column<long>(nullable: false),
                    TransactionRequiredConfirmationsCount = table.Column<long>(nullable: false),
                    TransactionDateTime = table.Column<DateTime>(nullable: false),
                    ErrorMessage = table.Column<string>(nullable: true),
                    ErrorCode = table.Column<int>(nullable: true),
                    DepositState = table.Column<int>(nullable: false),
                    Address = table.Column<string>(nullable: true),
                    DetectedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    ConfirmedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    CompletedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    FailedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    CancelledDateTime = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "deposit_fees",
                schema: "brokerage",
                columns: table => new
                {
                    DepositId = table.Column<long>(nullable: false),
                    TransferId = table.Column<long>(nullable: false),
                    AssetId = table.Column<long>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposit_fees", x => new { x.DepositId, x.TransferId });
                    table.ForeignKey(
                        name: "FK_deposit_fees_deposits_DepositId",
                        column: x => x.DepositId,
                        principalSchema: "brokerage",
                        principalTable: "deposits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "deposit_sources",
                schema: "brokerage",
                columns: table => new
                {
                    DepositId = table.Column<long>(nullable: false),
                    TransferId = table.Column<long>(nullable: false),
                    Address = table.Column<string>(nullable: true),
                    Amount = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposit_sources", x => new { x.DepositId, x.TransferId });
                    table.ForeignKey(
                        name: "FK_deposit_sources_deposits_DepositId",
                        column: x => x.DepositId,
                        principalSchema: "brokerage",
                        principalTable: "deposits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deposit_NaturalId",
                schema: "brokerage",
                table: "deposits",
                columns: new[] { "TransactionId", "AssetId", "BrokerAccountRequisitesId", "AccountRequisitesId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deposit_fees",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "deposit_sources",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "deposits",
                schema: "brokerage");
        }
    }
}
