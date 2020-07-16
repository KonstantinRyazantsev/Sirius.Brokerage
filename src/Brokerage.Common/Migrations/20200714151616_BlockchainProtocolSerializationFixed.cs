using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class BlockchainProtocolSerializationFixed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Blockchain_ProtocolCode",
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
                name: "ProtocolCode",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.DropColumn(
                name: "Protocol_Capabilities_DestinationTag_Text_MaxLength",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.AddColumn<string>(
                name: "Protocol",
                schema: "brokerage",
                table: "blockchains",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Protocol",
                schema: "brokerage",
                table: "blockchains");

            migrationBuilder.AddColumn<long>(
                name: "Protocol_Capabilities_DestinationTag_Number_Max",
                schema: "brokerage",
                table: "blockchains",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Protocol_Capabilities_DestinationTag_Number_Min",
                schema: "brokerage",
                table: "blockchains",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProtocolCode",
                schema: "brokerage",
                table: "blockchains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Protocol_Capabilities_DestinationTag_Text_MaxLength",
                schema: "brokerage",
                table: "blockchains",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blockchain_ProtocolCode",
                schema: "brokerage",
                table: "blockchains",
                column: "ProtocolCode");
        }
    }
}
