using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class MinDepositForConsolidationFixInEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MinDepositForConsolidation",
                schema: "brokerage",
                table: "deposits",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinDepositForConsolidation",
                schema: "brokerage",
                table: "deposits");
        }
    }
}
