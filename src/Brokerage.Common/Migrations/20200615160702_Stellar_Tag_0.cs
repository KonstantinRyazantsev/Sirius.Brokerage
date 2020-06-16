using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class Stellar_Tag_0 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccountDetails_NaturalId",
                schema: "brokerage",
                table: "account_details");

            migrationBuilder.AddColumn<long>(
                name: "ChainSequence",
                schema: "brokerage",
                table: "blockchains",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "brokerage",
                table: "blockchains",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "LatestBlockNumber",
                schema: "brokerage",
                table: "blockchains",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "brokerage",
                table: "blockchains",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NetworkType",
                schema: "brokerage",
                table: "blockchains",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "brokerage",
                table: "blockchains",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "brokerage",
                table: "blockchains",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "brokerage",
                table: "blockchains",
                type: "xid",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<long>(
                name: "Protocol_Capabilities_DestinationTag_Number_Max",
                schema: "brokerage",
                table: "blockchains",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Protocol_Capabilities_DestinationTag_Number_Min",
                schema: "brokerage",
                table: "blockchains",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Protocol_Capabilities_DestinationTag_Number_Name",
                schema: "brokerage",
                table: "blockchains",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProtocolCode",
                schema: "brokerage",
                table: "blockchains",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoubleSpendingProtectionType",
                schema: "brokerage",
                table: "blockchains",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProtocolName",
                schema: "brokerage",
                table: "blockchains",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StartBlockNumber",
                schema: "brokerage",
                table: "blockchains",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Protocol_Requirements_PublicKey",
                schema: "brokerage",
                table: "blockchains",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Protocol_Capabilities_DestinationTag_Text_MaxLength",
                schema: "brokerage",
                table: "blockchains",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Protocol_Capabilities_DestinationTag_Text_Name",
                schema: "brokerage",
                table: "blockchains",
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
                name: "IX_Blockchain_ProtocolCode",
                schema: "brokerage",
                table: "blockchains",
                column: "ProtocolCode");

            migrationBuilder.CreateIndex(
                name: "IX_Blockchain_ProtocolName",
                schema: "brokerage",
                table: "blockchains",
                column: "ProtocolName");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDetails_NaturalId",
                schema: "brokerage",
                table: "account_details",
                column: "NaturalId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "IX_Blockchain_ProtocolCode",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropIndex(
                name: "IX_Blockchain_ProtocolName",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropIndex(
                name: "IX_AccountDetails_NaturalId",
                schema: "brokerage",
                table: "account_details");

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
                name: "xmin",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "Protocol_Capabilities_DestinationTag_Number_Max",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "Protocol_Capabilities_DestinationTag_Number_Min",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "Protocol_Capabilities_DestinationTag_Number_Name",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "ProtocolCode",
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
                name: "Protocol_Capabilities_DestinationTag_Text_MaxLength",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "Protocol_Capabilities_DestinationTag_Text_Name",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDetails_NaturalId",
                schema: "brokerage",
                table: "account_details",
                column: "NaturalId");
        }
    }
}
