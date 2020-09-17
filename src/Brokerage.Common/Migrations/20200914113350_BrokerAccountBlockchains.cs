using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class BrokerAccountBlockchains : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlockchainIds",
                schema: "brokerage",
                table: "broker_accounts",
                nullable: true);

            migrationBuilder.Sql($@"
                            UPDATE brokerage.broker_accounts 
                            SET ""BlockchainIds"" = (SELECT FORMAT('[ %s ]',string_agg(FORMAT('""%s""',""Id""), ', ')) FROM brokerage.blockchains) 
                            WHERE ""BlockchainIds"" is null;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockchainIds",
                schema: "brokerage",
                table: "broker_accounts");
        }
    }
}
