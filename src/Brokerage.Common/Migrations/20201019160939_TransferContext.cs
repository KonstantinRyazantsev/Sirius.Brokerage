using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class TransferContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "withdrawals_fees",
                schema: "brokerage");

            migrationBuilder.DropColumn(
                name: "UserContext",
                schema: "brokerage",
                table: "withdrawals");

            migrationBuilder.AddColumn<string>(
                name: "TransferContext",
                schema: "brokerage",
                table: "withdrawals",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransferContext",
                schema: "brokerage",
                table: "withdrawals");

            migrationBuilder.AddColumn<string>(
                name: "UserContext",
                schema: "brokerage",
                table: "withdrawals",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "withdrawals_fees",
                schema: "brokerage",
                columns: table => new
                {
                    WithdrawalId = table.Column<long>(type: "bigint", nullable: false),
                    AssetId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    WithdrawalEntityId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_withdrawals_fees", x => new { x.WithdrawalId, x.AssetId });
                    table.ForeignKey(
                        name: "FK_withdrawals_fees_withdrawals_WithdrawalEntityId",
                        column: x => x.WithdrawalEntityId,
                        principalSchema: "brokerage",
                        principalTable: "withdrawals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_withdrawals_fees_WithdrawalEntityId",
                schema: "brokerage",
                table: "withdrawals_fees",
                column: "WithdrawalEntityId");
        }
    }
}
