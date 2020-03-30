using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brokerage.Bilv1.Repositories.Migrations
{
    public partial class WalletMAnager : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "enrolled_balance",
                schema: "blockchain_wallet_api",
                columns: table => new
                {
                    BlockchianId = table.Column<string>(nullable: false),
                    BlockchainAssetId = table.Column<string>(nullable: false),
                    WalletAddress = table.Column<string>(nullable: false),
                    OriginalWalletAddress = table.Column<string>(nullable: true),
                    BlockNumber = table.Column<long>(nullable: false),
                    Balance = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrolled_balance", x => new { x.BlockchianId, x.BlockchainAssetId, x.WalletAddress });
                });

            migrationBuilder.CreateTable(
                name: "operations",
                schema: "blockchain_wallet_api",
                columns: table => new
                {
                    BlockchianId = table.Column<string>(nullable: false),
                    BlockchainAssetId = table.Column<string>(nullable: false),
                    WalletAddress = table.Column<string>(nullable: false),
                    OperationId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OriginalWalletAddress = table.Column<string>(nullable: true),
                    BlockNumber = table.Column<long>(nullable: false),
                    BalanceChange = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operations", x => new { x.BlockchianId, x.BlockchainAssetId, x.WalletAddress, x.OperationId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enrolled_balance",
                schema: "blockchain_wallet_api");

            migrationBuilder.DropTable(
                name: "operations",
                schema: "blockchain_wallet_api");
        }
    }
}
