using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class MinDepositForConsolidationFixInEntityNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                            UPDATE brokerage.deposits 
                            SET ""MinDepositForConsolidation"" = 0 
                            WHERE ""MinDepositForConsolidation"" is null;");

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
