using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class BrokerAccountIdAsIndexRenamed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_broker_account_requisites_BrokerAccountId",
                schema: "brokerage",
                table: "broker_account_requisites",
                newName: "IX_BrokerAccountRequisites_BrokerAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_BrokerAccountRequisites_BrokerAccountId",
                schema: "brokerage",
                table: "broker_account_requisites",
                newName: "IX_broker_account_requisites_BrokerAccountId");
        }
    }
}
