using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brokerage.Common.Migrations
{
    public partial class FinalizeBrokerAccountCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "broker_account_requisites",
                schema: "brokerage",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestId = table.Column<string>(nullable: true),
                    BlockchainId = table.Column<string>(nullable: true),
                    BrokerAccountId = table.Column<long>(nullable: false),
                    Address = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_broker_account_requisites", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrokerAccountRequisites_RequestId",
                schema: "brokerage",
                table: "broker_account_requisites",
                column: "RequestId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "broker_account_requisites",
                schema: "brokerage");
        }
    }
}
