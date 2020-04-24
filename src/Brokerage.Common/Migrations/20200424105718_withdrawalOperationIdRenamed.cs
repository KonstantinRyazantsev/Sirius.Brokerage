using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class withdrawalOperationIdRenamed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Withdrawal_OperationId",
                schema: "brokerage",
                table: "withdrawals");

            migrationBuilder.RenameColumn(
                name: "WithdrawalOperationId",
                schema: "brokerage",
                table: "withdrawals",
                newName: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_Withdrawal_OperationId",
                schema: "brokerage",
                table: "withdrawals",
                column: "OperationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Withdrawal_OperationId",
                schema: "brokerage",
                table: "withdrawals");

            migrationBuilder.RenameColumn(
                name: "OperationId",
                schema: "brokerage",
                table: "withdrawals",
                newName: "WithdrawalOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_Withdrawal_OperationId",
                schema: "brokerage",
                table: "withdrawals",
                column: "WithdrawalOperationId");
        }
    }
}
