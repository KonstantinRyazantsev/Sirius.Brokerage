using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brokerage.Common.Migrations
{
    public partial class MinDepositResidualsFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MinDepositResiduals_DepositId",
                schema: "brokerage",
                table: "min_deposit_residuals");

            migrationBuilder.AlterColumn<long>(
                name: "DepositId",
                schema: "brokerage",
                table: "min_deposit_residuals",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_min_deposit_residuals",
                schema: "brokerage",
                table: "min_deposit_residuals",
                column: "DepositId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_min_deposit_residuals",
                schema: "brokerage",
                table: "min_deposit_residuals");

            migrationBuilder.AlterColumn<long>(
                name: "DepositId",
                schema: "brokerage",
                table: "min_deposit_residuals",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long))
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_MinDepositResiduals_DepositId",
                schema: "brokerage",
                table: "min_deposit_residuals",
                column: "DepositId",
                unique: true);
        }
    }
}
