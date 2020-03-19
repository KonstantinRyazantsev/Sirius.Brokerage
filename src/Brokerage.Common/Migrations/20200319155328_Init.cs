using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brokerage.Common.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "brokerage");

            migrationBuilder.CreateTable(
                name: "blockchains",
                schema: "brokerage",
                columns: table => new
                {
                    BlockchainId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blockchains", x => x.BlockchainId);
                });

            migrationBuilder.CreateTable(
                name: "broker_accounts",
                schema: "brokerage",
                columns: table => new
                {
                    BlockchainId = table.Column<string>(nullable: false),
                    NetworkId = table.Column<string>(nullable: false),
                    BrokerAccountId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestId = table.Column<string>(nullable: true),
                    TenantId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_broker_accounts", x => new { x.BlockchainId, x.NetworkId, x.BrokerAccountId });
                });

            migrationBuilder.CreateTable(
                name: "networks",
                schema: "brokerage",
                columns: table => new
                {
                    BlockchainId = table.Column<string>(nullable: false),
                    NetworkId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_networks", x => new { x.BlockchainId, x.NetworkId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrokerAccount_RequestId",
                schema: "brokerage",
                table: "broker_accounts",
                column: "RequestId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blockchains",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "broker_accounts",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "networks",
                schema: "brokerage");
        }
    }
}
