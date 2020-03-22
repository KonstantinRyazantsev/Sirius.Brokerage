using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class ProtocolScheme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "networks",
                schema: "brokerage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_broker_accounts",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropColumn(
                name: "BlockchainId",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropColumn(
                name: "NetworkId",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivationDateTime",
                schema: "brokerage",
                table: "broker_accounts",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BlockingDateTime",
                schema: "brokerage",
                table: "broker_accounts",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDateTime",
                schema: "brokerage",
                table: "broker_accounts",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "State",
                schema: "brokerage",
                table: "broker_accounts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_broker_accounts",
                schema: "brokerage",
                table: "broker_accounts",
                column: "BrokerAccountId");

            migrationBuilder.CreateTable(
                name: "protocols",
                schema: "brokerage",
                columns: table => new
                {
                    ProtocolId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_protocols", x => x.ProtocolId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "protocols",
                schema: "brokerage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_broker_accounts",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropColumn(
                name: "ActivationDateTime",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropColumn(
                name: "BlockingDateTime",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropColumn(
                name: "CreationDateTime",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropColumn(
                name: "State",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.AddColumn<string>(
                name: "BlockchainId",
                schema: "brokerage",
                table: "broker_accounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NetworkId",
                schema: "brokerage",
                table: "broker_accounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_broker_accounts",
                schema: "brokerage",
                table: "broker_accounts",
                columns: new[] { "BlockchainId", "NetworkId", "BrokerAccountId" });

            migrationBuilder.CreateTable(
                name: "networks",
                schema: "brokerage",
                columns: table => new
                {
                    BlockchainId = table.Column<string>(type: "text", nullable: false),
                    NetworkId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_networks", x => new { x.BlockchainId, x.NetworkId });
                });
        }
    }
}
