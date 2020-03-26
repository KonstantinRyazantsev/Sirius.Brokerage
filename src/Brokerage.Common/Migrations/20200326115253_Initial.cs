using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brokerage.Common.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "brokerage");

            migrationBuilder.CreateTable(
                name: "blockchains",
                schema: "brokerage",
                columns: table => new
                {
                    BlockchainId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blockchains", x => x.BlockchainId);
                });

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

            migrationBuilder.CreateTable(
                name: "broker_accounts",
                schema: "brokerage",
                columns: table => new
                {
                    BrokerAccountId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestId = table.Column<string>(nullable: true),
                    TenantId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    CreationDateTime = table.Column<DateTimeOffset>(nullable: false),
                    ActivationDateTime = table.Column<DateTimeOffset>(nullable: true),
                    BlockingDateTime = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_broker_accounts", x => x.BrokerAccountId);
                });

            migrationBuilder.CreateTable(
                name: "protocols",
                schema: "brokerage",
                columns: table => new
                {
                    ProtocolId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_protocols", x => x.ProtocolId);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                schema: "brokerage",
                columns: table => new
                {
                    AccountId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestId = table.Column<string>(nullable: true),
                    BrokerAccountId = table.Column<long>(nullable: false),
                    ReferenceId = table.Column<string>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    CreationDateTime = table.Column<DateTimeOffset>(nullable: false),
                    ActivationDateTime = table.Column<DateTimeOffset>(nullable: true),
                    BlockingDateTime = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_accounts_broker_accounts_BrokerAccountId",
                        column: x => x.BrokerAccountId,
                        principalSchema: "brokerage",
                        principalTable: "broker_accounts",
                        principalColumn: "BrokerAccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_requisites",
                schema: "brokerage",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestId = table.Column<string>(nullable: true),
                    AccountId = table.Column<long>(nullable: false),
                    BlockchainId = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Tag = table.Column<string>(nullable: true),
                    TagType = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_requisites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_account_requisites_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "brokerage",
                        principalTable: "accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_requisites_AccountId",
                schema: "brokerage",
                table: "account_requisites",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRequisites_RequestId",
                schema: "brokerage",
                table: "account_requisites",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounts_BrokerAccountId",
                schema: "brokerage",
                table: "accounts",
                column: "BrokerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Account_RequestId",
                schema: "brokerage",
                table: "accounts",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BrokerAccountRequisites_RequestId",
                schema: "brokerage",
                table: "broker_account_requisites",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BrokerAccount_RequestId",
                schema: "brokerage",
                table: "broker_accounts",
                column: "RequestId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_requisites",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "blockchains",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "broker_account_requisites",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "protocols",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "accounts",
                schema: "brokerage");

            migrationBuilder.DropTable(
                name: "broker_accounts",
                schema: "brokerage");
        }
    }
}
