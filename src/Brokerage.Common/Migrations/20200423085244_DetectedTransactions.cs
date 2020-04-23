using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class DetectedTransactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "detected_transactions",
                schema: "brokerage",
                columns: table => new
                {
                    BlockchainId = table.Column<string>(nullable: false),
                    TransactionId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detected_transactions", x => new { x.BlockchainId, x.TransactionId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "detected_transactions",
                schema: "brokerage");
        }
    }
}
