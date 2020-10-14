using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class DepositTypes : Migration
    {
        /*
            TinyDeposit = 0,
            BrokerDeposit = 1,
            RegularDeposit = 2,
            TokenDeposit = 3,
         */
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepositType",
                schema: "brokerage",
                table: "deposits",
                nullable: false,
                defaultValue:0);

            migrationBuilder.Sql(@" UPDATE brokerage.deposits
                                    SET ""DepositType"" = 2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepositType",
                schema: "brokerage",
                table: "deposits");
        }
    }
}
