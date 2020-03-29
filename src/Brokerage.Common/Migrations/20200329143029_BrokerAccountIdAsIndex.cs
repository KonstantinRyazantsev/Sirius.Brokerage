using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class BrokerAccountIdAsIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_broker_account_requisites_BrokerAccountId",
                schema: "brokerage",
                table: "broker_account_requisites",
                column: "BrokerAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_broker_account_requisites_BrokerAccountId",
                schema: "brokerage",
                table: "broker_account_requisites");
        }
    }
}
