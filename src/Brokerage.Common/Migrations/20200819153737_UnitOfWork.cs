using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Brokerage.Common.Migrations
{
    public partial class UnitOfWork : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from brokerage.outbox");

            migrationBuilder.DropTable(
                name: "broker_account_balances_update",
                schema: "brokerage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_outbox",
                schema: "brokerage",
                table: "outbox");

            migrationBuilder.DropIndex(
                name: "IX_BrokerAccount_RequestId",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropColumn(
                name: "RequestId",
                schema: "brokerage",
                table: "outbox");

            migrationBuilder.DropColumn(
                name: "AggregateId",
                schema: "brokerage",
                table: "outbox");

            migrationBuilder.DropColumn(
                name: "IsStored",
                schema: "brokerage",
                table: "outbox");

            migrationBuilder.DropColumn(
                name: "RequestId",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.CreateSequence(
                name: "id_generator_account_details",
                schema: "brokerage",
                startValue: 104000000L);

            migrationBuilder.CreateSequence(
                name: "id_generator_accounts",
                schema: "brokerage",
                startValue: 103000000L);

            migrationBuilder.CreateSequence(
                name: "id_generator_broker_account_balances",
                schema: "brokerage",
                startValue: 102000000L);

            migrationBuilder.CreateSequence(
                name: "id_generator_broker_account_details",
                schema: "brokerage",
                startValue: 101000000L);

            migrationBuilder.CreateSequence(
                name: "id_generator_broker_accounts",
                schema: "brokerage",
                startValue: 100000000L);

            migrationBuilder.CreateSequence(
                name: "id_generator_deposits",
                schema: "brokerage",
                startValue: 105000000L);

            migrationBuilder.CreateSequence(
                name: "id_generator_withdrawals",
                schema: "brokerage",
                startValue: 106000000L);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "withdrawals",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:IdentitySequenceOptions", "'10600000', '1', '', '', 'False', '1'")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyId",
                schema: "brokerage",
                table: "outbox",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "deposits",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:IdentitySequenceOptions", "'10500000', '1', '', '', 'False', '1'")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "broker_accounts",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:IdentitySequenceOptions", "'10000000', '1', '', '', 'False', '1'")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "broker_account_details",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:IdentitySequenceOptions", "'10100000', '1', '', '', 'False', '1'")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "broker_account_balances",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:IdentitySequenceOptions", "'10400000', '1', '', '', 'False', '1'")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "accounts",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:IdentitySequenceOptions", "'10200000', '1', '', '', 'False', '1'")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "account_details",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:IdentitySequenceOptions", "'10300000', '1', '', '', 'False', '1'")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_outbox",
                schema: "brokerage",
                table: "outbox",
                column: "IdempotencyId");

            migrationBuilder.CreateTable(
                name: "id_generator",
                schema: "brokerage",
                columns: table => new
                {
                    IdempotencyId = table.Column<string>(nullable: false),
                    Value = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_id_generator", x => x.IdempotencyId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "id_generator",
                schema: "brokerage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_outbox",
                schema: "brokerage",
                table: "outbox");

            migrationBuilder.DropSequence(
                name: "id_generator_account_details",
                schema: "brokerage");

            migrationBuilder.DropSequence(
                name: "id_generator_accounts",
                schema: "brokerage");

            migrationBuilder.DropSequence(
                name: "id_generator_broker_account_balances",
                schema: "brokerage");

            migrationBuilder.DropSequence(
                name: "id_generator_broker_account_details",
                schema: "brokerage");

            migrationBuilder.DropSequence(
                name: "id_generator_broker_accounts",
                schema: "brokerage");

            migrationBuilder.DropSequence(
                name: "id_generator_deposits",
                schema: "brokerage");

            migrationBuilder.DropSequence(
                name: "id_generator_withdrawals",
                schema: "brokerage");

            migrationBuilder.DropColumn(
                name: "IdempotencyId",
                schema: "brokerage",
                table: "outbox");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "withdrawals",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("Npgsql:IdentitySequenceOptions", "'10600000', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "RequestId",
                schema: "brokerage",
                table: "outbox",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "AggregateId",
                schema: "brokerage",
                table: "outbox",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:IdentitySequenceOptions", "'2', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<bool>(
                name: "IsStored",
                schema: "brokerage",
                table: "outbox",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "deposits",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("Npgsql:IdentitySequenceOptions", "'10500000', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "broker_accounts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("Npgsql:IdentitySequenceOptions", "'10000000', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "RequestId",
                schema: "brokerage",
                table: "broker_accounts",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "broker_account_details",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("Npgsql:IdentitySequenceOptions", "'10100000', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "broker_account_balances",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("Npgsql:IdentitySequenceOptions", "'10400000', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "accounts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("Npgsql:IdentitySequenceOptions", "'10200000', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "brokerage",
                table: "account_details",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("Npgsql:IdentitySequenceOptions", "'10300000', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_outbox",
                schema: "brokerage",
                table: "outbox",
                column: "RequestId");

            migrationBuilder.CreateTable(
                name: "broker_account_balances_update",
                schema: "brokerage",
                columns: table => new
                {
                    UpdateId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_broker_account_balances_update", x => x.UpdateId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrokerAccount_RequestId",
                schema: "brokerage",
                table: "broker_accounts",
                column: "RequestId",
                unique: true);
        }
    }
}
