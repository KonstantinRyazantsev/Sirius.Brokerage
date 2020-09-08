using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class MinDepositResiduals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "min_deposit_residuals",
                schema: "brokerage",
                columns: table => new
                {
                    NaturalId = table.Column<string>(nullable: true),
                    AssetId = table.Column<long>(nullable: false),
                    BlockchainId = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Tag = table.Column<string>(nullable: true),
                    TagType = table.Column<int>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    DepositId = table.Column<long>(nullable: false),
                    ConsolidationDepositId = table.Column<long>(nullable: true),
                    xmin = table.Column<uint>(type: "xid", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_MinDepositResiduals_ConsolidationDepositId",
                schema: "brokerage",
                table: "min_deposit_residuals",
                column: "ConsolidationDepositId");

            migrationBuilder.CreateIndex(
                name: "IX_MinDepositResiduals_DepositId",
                schema: "brokerage",
                table: "min_deposit_residuals",
                column: "DepositId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MinDepositResiduals_NaturalId",
                schema: "brokerage",
                table: "min_deposit_residuals",
                column: "NaturalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "min_deposit_residuals",
                schema: "brokerage");
        }
    }
}
