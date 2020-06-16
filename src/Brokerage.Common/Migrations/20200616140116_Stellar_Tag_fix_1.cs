using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class Stellar_Tag_fix_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Blockchain_ChainSequence",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropIndex(
                name: "IX_Blockchain_Name",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropIndex(
                name: "IX_Blockchain_NetworkType",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropIndex(
                name: "IX_Blockchain_TenantId",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropIndex(
                name: "IX_Blockchain_ProtocolName",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "ChainSequence",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "LatestBlockNumber",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "NetworkType",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "Protocol_Capabilities_DestinationTag_Number_Name",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "DoubleSpendingProtectionType",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "ProtocolName",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "StartBlockNumber",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "Protocol_Requirements_PublicKey",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "Protocol_Capabilities_DestinationTag_Text_Name",
                schema: "brokerage",
                table: "blockchains");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChainSequence",
                schema: "brokerage",
                table: "blockchains",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "brokerage",
                table: "blockchains",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "LatestBlockNumber",
                schema: "brokerage",
                table: "blockchains",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "brokerage",
                table: "blockchains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NetworkType",
                schema: "brokerage",
                table: "blockchains",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "brokerage",
                table: "blockchains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "brokerage",
                table: "blockchains",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Protocol_Capabilities_DestinationTag_Number_Name",
                schema: "brokerage",
                table: "blockchains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoubleSpendingProtectionType",
                schema: "brokerage",
                table: "blockchains",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProtocolName",
                schema: "brokerage",
                table: "blockchains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StartBlockNumber",
                schema: "brokerage",
                table: "blockchains",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Protocol_Requirements_PublicKey",
                schema: "brokerage",
                table: "blockchains",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Protocol_Capabilities_DestinationTag_Text_Name",
                schema: "brokerage",
                table: "blockchains",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blockchain_ChainSequence",
                schema: "brokerage",
                table: "blockchains",
                column: "ChainSequence");

            migrationBuilder.CreateIndex(
                name: "IX_Blockchain_Name",
                schema: "brokerage",
                table: "blockchains",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Blockchain_NetworkType",
                schema: "brokerage",
                table: "blockchains",
                column: "NetworkType");

            migrationBuilder.CreateIndex(
                name: "IX_Blockchain_TenantId",
                schema: "brokerage",
                table: "blockchains",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Blockchain_ProtocolName",
                schema: "brokerage",
                table: "blockchains",
                column: "ProtocolName");
        }
    }
}
