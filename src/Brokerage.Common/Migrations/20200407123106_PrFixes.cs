using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class PrFixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_deposit_sources",
                schema: "brokerage",
                table: "deposit_sources");

            migrationBuilder.DropPrimaryKey(
                name: "PK_deposit_fees",
                schema: "brokerage",
                table: "deposit_fees");

            migrationBuilder.DropColumn(
                name: "TransferId",
                schema: "brokerage",
                table: "deposit_sources");

            migrationBuilder.DropColumn(
                name: "TransferId",
                schema: "brokerage",
                table: "deposit_fees");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                schema: "brokerage",
                table: "deposit_sources",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_deposit_sources",
                schema: "brokerage",
                table: "deposit_sources",
                columns: new[] { "DepositId", "Address" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_deposit_fees",
                schema: "brokerage",
                table: "deposit_fees",
                columns: new[] { "DepositId", "AssetId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_deposit_sources",
                schema: "brokerage",
                table: "deposit_sources");

            migrationBuilder.DropPrimaryKey(
                name: "PK_deposit_fees",
                schema: "brokerage",
                table: "deposit_fees");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                schema: "brokerage",
                table: "deposit_sources",
                type: "text",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<long>(
                name: "TransferId",
                schema: "brokerage",
                table: "deposit_sources",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TransferId",
                schema: "brokerage",
                table: "deposit_fees",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_deposit_sources",
                schema: "brokerage",
                table: "deposit_sources",
                columns: new[] { "DepositId", "TransferId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_deposit_fees",
                schema: "brokerage",
                table: "deposit_fees",
                columns: new[] { "DepositId", "TransferId" });
        }
    }
}
