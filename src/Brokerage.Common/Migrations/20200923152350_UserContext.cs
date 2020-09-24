using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class UserContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("ReferenceId", "withdrawals", "AccountReferenceId", "brokerage");

            migrationBuilder.AddColumn<string>(
                name: "UserContext",
                schema: "brokerage",
                table: "withdrawals",
                nullable: true);

            migrationBuilder.Sql(@"
                            UPDATE brokerage.withdrawals 
                            SET ""UserContext"" = '{}' 
                            WHERE ""UserContext"" is null;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("AccountReferenceId", "withdrawals", "ReferenceId", "brokerage");

            migrationBuilder.DropColumn(
                name: "UserContext",
                schema: "brokerage",
                table: "withdrawals");
        }
    }
}
