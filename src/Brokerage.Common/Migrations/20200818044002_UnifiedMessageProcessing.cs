using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class UnifiedMessageProcessing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Sequence",
                schema: "brokerage",
                table: "broker_accounts",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Sequence",
                schema: "brokerage",
                table: "accounts",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sequence",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropColumn(
                name: "Sequence",
                schema: "brokerage",
                table: "accounts");
        }
    }
}
