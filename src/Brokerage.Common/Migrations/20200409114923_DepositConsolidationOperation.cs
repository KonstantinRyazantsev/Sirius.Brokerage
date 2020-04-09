using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class DepositConsolidationOperation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Deposit_OperationId",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.DropColumn(
                name: "OperationId",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.AddColumn<long>(
                name: "ConsolidationOperationId",
                schema: "brokerage",
                table: "deposits",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deposit_ConsolidationOperationId",
                schema: "brokerage",
                table: "deposits",
                column: "ConsolidationOperationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Deposit_ConsolidationOperationId",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.DropColumn(
                name: "ConsolidationOperationId",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.AddColumn<long>(
                name: "OperationId",
                schema: "brokerage",
                table: "deposits",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deposit_OperationId",
                schema: "brokerage",
                table: "deposits",
                column: "OperationId");
        }
    }
}
