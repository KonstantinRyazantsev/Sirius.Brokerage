using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class AccountDetailsAccountIdBlockchainIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccountDetails_AccountId",
                schema: "brokerage",
                table: "account_requisites");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDetails_AccountId_BlockchainId",
                schema: "brokerage",
                table: "account_requisites",
                columns: new[] { "AccountId", "BlockchainId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccountDetails_AccountId_BlockchainId",
                schema: "brokerage",
                table: "account_requisites");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDetails_AccountId",
                schema: "brokerage",
                table: "account_requisites",
                column: "AccountId",
                unique: true);
        }
    }
}
