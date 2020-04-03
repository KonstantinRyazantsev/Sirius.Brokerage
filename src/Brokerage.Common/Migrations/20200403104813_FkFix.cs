using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class FkFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_broker_account_balances_BrokerAccountId",
                schema: "brokerage",
                table: "broker_account_balances");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_broker_account_balances_BrokerAccountId",
                schema: "brokerage",
                table: "broker_account_balances",
                column: "BrokerAccountId",
                unique: true);
        }
    }
}
