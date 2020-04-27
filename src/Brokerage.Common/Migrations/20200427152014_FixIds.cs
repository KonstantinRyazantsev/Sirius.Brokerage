using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brokerage.Common.Migrations
{
    public partial class FixIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_account_requisites_accounts_AccountId",
                schema: "brokerage",
                table: "account_requisites");

            migrationBuilder.DropForeignKey(
                name: "FK_accounts_broker_accounts_BrokerAccountId",
                schema: "brokerage",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_broker_account_balances_broker_accounts_BrokerAccountId",
                schema: "brokerage",
                table: "broker_account_balances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_broker_accounts",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_accounts",
                schema: "brokerage",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "BrokerAccountId",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropColumn(
                name: "AccountId",
                schema: "brokerage",
                table: "accounts");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "broker_accounts",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:IdentitySequenceOptions", "'10000000', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "accounts",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:IdentitySequenceOptions", "'10200000', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_broker_accounts",
                schema: "brokerage",
                table: "broker_accounts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_accounts",
                schema: "brokerage",
                table: "accounts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_account_requisites_accounts_AccountId",
                schema: "brokerage",
                table: "account_requisites",
                column: "AccountId",
                principalSchema: "brokerage",
                principalTable: "accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_accounts_broker_accounts_BrokerAccountId",
                schema: "brokerage",
                table: "accounts",
                column: "BrokerAccountId",
                principalSchema: "brokerage",
                principalTable: "broker_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_broker_account_balances_broker_accounts_BrokerAccountId",
                schema: "brokerage",
                table: "broker_account_balances",
                column: "BrokerAccountId",
                principalSchema: "brokerage",
                principalTable: "broker_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_account_requisites_accounts_AccountId",
                schema: "brokerage",
                table: "account_requisites");

            migrationBuilder.DropForeignKey(
                name: "FK_accounts_broker_accounts_BrokerAccountId",
                schema: "brokerage",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_broker_account_balances_broker_accounts_BrokerAccountId",
                schema: "brokerage",
                table: "broker_account_balances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_broker_accounts",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_accounts",
                schema: "brokerage",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "brokerage",
                table: "accounts");

            migrationBuilder.AddColumn<long>(
                name: "BrokerAccountId",
                schema: "brokerage",
                table: "broker_accounts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:IdentitySequenceOptions", "'10000000', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "AccountId",
                schema: "brokerage",
                table: "accounts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:IdentitySequenceOptions", "'10200000', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_broker_accounts",
                schema: "brokerage",
                table: "broker_accounts",
                column: "BrokerAccountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_accounts",
                schema: "brokerage",
                table: "accounts",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_account_requisites_accounts_AccountId",
                schema: "brokerage",
                table: "account_requisites",
                column: "AccountId",
                principalSchema: "brokerage",
                principalTable: "accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_accounts_broker_accounts_BrokerAccountId",
                schema: "brokerage",
                table: "accounts",
                column: "BrokerAccountId",
                principalSchema: "brokerage",
                principalTable: "broker_accounts",
                principalColumn: "BrokerAccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_broker_account_balances_broker_accounts_BrokerAccountId",
                schema: "brokerage",
                table: "broker_account_balances",
                column: "BrokerAccountId",
                principalSchema: "brokerage",
                principalTable: "broker_accounts",
                principalColumn: "BrokerAccountId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
