using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Bilv1.Repositories.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "blockchain_wallet_api");

            migrationBuilder.CreateTable(
                name: "wallets",
                schema: "blockchain_wallet_api",
                columns: table => new
                {
                    BlockchainId = table.Column<string>(nullable: false),
                    NetworkId = table.Column<string>(nullable: false),
                    Id = table.Column<Guid>(nullable: false),
                    WalletAddress = table.Column<string>(nullable: true),
                    OriginalWalletAddress = table.Column<string>(nullable: true),
                    PublicKey = table.Column<string>(nullable: true),
                    IsCompromised = table.Column<bool>(nullable: false),
                    ImportedDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallets", x => new { x.BlockchainId, x.NetworkId, x.Id });
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainId_NetworkId_WalletAddress",
                schema: "blockchain_wallet_api",
                table: "wallets",
                columns: new[] { "BlockchainId", "NetworkId", "WalletAddress" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wallets",
                schema: "blockchain_wallet_api");
        }
    }
}
