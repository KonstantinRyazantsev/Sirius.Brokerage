using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brokerage.Common.Migrations
{
    public partial class BilV1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IntegrationUrl",
                schema: "brokerage",
                table: "blockchains",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "broker_account_balances",
                schema: "brokerage",
                columns: table => new
                {
                    BrokerAccountBalancesId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Version = table.Column<long>(nullable: false),
                    BrokerAccountId = table.Column<long>(nullable: false),
                    AssetId = table.Column<long>(nullable: false),
                    OwnedBalance = table.Column<decimal>(nullable: false),
                    AvailableBalance = table.Column<decimal>(nullable: false),
                    PendingBalance = table.Column<decimal>(nullable: false),
                    ReservedBalance = table.Column<decimal>(nullable: false),
                    OwnedBalanceUpdateDateTime = table.Column<DateTimeOffset>(nullable: false),
                    AvailableBalanceUpdateDateTime = table.Column<DateTimeOffset>(nullable: false),
                    PendingBalanceUpdateDateTime = table.Column<DateTimeOffset>(nullable: false),
                    ReservedBalanceUpdateDateTime = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_broker_account_balances", x => x.BrokerAccountBalancesId);
                    table.ForeignKey(
                        name: "FK_broker_account_balances_broker_accounts_BrokerAccountId",
                        column: x => x.BrokerAccountId,
                        principalSchema: "brokerage",
                        principalTable: "broker_accounts",
                        principalColumn: "BrokerAccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_broker_account_balances_BrokerAccountId",
                schema: "brokerage",
                table: "broker_account_balances",
                column: "BrokerAccountId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "broker_account_balances",
                schema: "brokerage");

            migrationBuilder.DropColumn(
                name: "IntegrationUrl",
                schema: "brokerage",
                table: "blockchains");
        }
    }
}
