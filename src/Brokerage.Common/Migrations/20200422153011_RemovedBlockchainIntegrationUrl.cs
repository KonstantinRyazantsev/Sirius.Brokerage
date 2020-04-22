using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class RemovedBlockchainIntegrationUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntegrationUrl",
                schema: "brokerage",
                table: "blockchains");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IntegrationUrl",
                schema: "brokerage",
                table: "blockchains",
                type: "text",
                nullable: true);
        }
    }
}
