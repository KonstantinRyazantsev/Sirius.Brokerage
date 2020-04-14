using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class BalancesUniqueIdx : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BrokerAccountBalances_BrokerAccountId_AssetId",
                schema: "brokerage",
                table: "broker_account_balances");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerAccountBalances_BrokerAccountId_AssetId",
                schema: "brokerage",
                table: "broker_account_balances",
                columns: new[] { "BrokerAccountId", "AssetId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BrokerAccountBalances_BrokerAccountId_AssetId",
                schema: "brokerage",
                table: "broker_account_balances");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerAccountBalances_BrokerAccountId_AssetId",
                schema: "brokerage",
                table: "broker_account_balances",
                columns: new[] { "BrokerAccountId", "AssetId" });
        }
    }
}
