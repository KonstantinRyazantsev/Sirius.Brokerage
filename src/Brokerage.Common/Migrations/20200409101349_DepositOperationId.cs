using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class DepositOperationId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OperationId",
                schema: "brokerage",
                table: "deposits",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deposit_OperationId",
                schema: "brokerage",
                table: "deposits",
                column: "OperationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Deposit_OperationId",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.DropColumn(
                name: "OperationId",
                schema: "brokerage",
                table: "deposits");
        }
    }
}
