using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class DepositsIndexedImprovedForSearch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Deposit_ConsolidationOperationId",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.DropIndex(
                name: "IX_Deposit_BlockchainId_TransactionId",
                schema: "brokerage",
                table: "deposits");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Deposit_BlockchainId",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.DropIndex(
                name: "IX_Deposit_ConsolidationOperationId",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.DropIndex(
                name: "IX_Deposit_TransactionId",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.CreateIndex(
                name: "IX_Deposit_ConsolidationOperationId",
                schema: "brokerage",
                table: "deposits",
                column: "ConsolidationOperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deposit_BlockchainId_TransactionId",
                schema: "brokerage",
                table: "deposits",
                columns: new[] { "BlockchainId", "TransactionId" });
        }
    }
}
