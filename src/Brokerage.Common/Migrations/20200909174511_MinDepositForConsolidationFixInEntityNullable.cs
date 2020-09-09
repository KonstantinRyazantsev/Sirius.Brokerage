using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class MinDepositForConsolidationFixInEntityNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MinDepositForConsolidation",
                schema: "brokerage",
                table: "deposits",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MinDepositForConsolidation",
                schema: "brokerage",
                table: "deposits",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal));
        }
    }
}
