using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class WithdrawalDelAccountReferenceId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountReferenceId",
                schema: "brokerage",
                table: "withdrawals");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountReferenceId",
                schema: "brokerage",
                table: "withdrawals",
                type: "text",
                nullable: true);
        }
    }
}
