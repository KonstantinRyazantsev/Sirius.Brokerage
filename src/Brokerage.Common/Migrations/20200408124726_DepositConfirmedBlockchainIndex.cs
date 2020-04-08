using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class DepositConfirmedBlockchainIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BrokerAccountRequisites_BlockchainId",
                schema: "brokerage",
                table: "broker_account_requisites",
                column: "BlockchainId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BrokerAccountRequisites_BlockchainId",
                schema: "brokerage",
                table: "broker_account_requisites");
        }
    }
}
