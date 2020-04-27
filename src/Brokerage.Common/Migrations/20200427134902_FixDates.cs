using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class FixDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDateTime",
                schema: "brokerage",
                table: "withdrawals");

            migrationBuilder.DropColumn(
                name: "UpdatedDateTime",
                schema: "brokerage",
                table: "withdrawals");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.DropColumn(
                name: "DetectedAt",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.DropColumn(
                name: "FailedAt",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.DropColumn(
                name: "ActivationDateTime",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropColumn(
                name: "BlockingDateTime",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropColumn(
                name: "CreationDateTime",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropColumn(
                name: "AvailableBalanceUpdatedAt",
                schema: "brokerage",
                table: "broker_account_balances");

            migrationBuilder.DropColumn(
                name: "OwnedBalanceUpdatedAt",
                schema: "brokerage",
                table: "broker_account_balances");

            migrationBuilder.DropColumn(
                name: "PendingBalanceUpdatedAt",
                schema: "brokerage",
                table: "broker_account_balances");

            migrationBuilder.DropColumn(
                name: "ReservedBalanceUpdateDatedAt",
                schema: "brokerage",
                table: "broker_account_balances");

            migrationBuilder.DropColumn(
                name: "ActivationDateTime",
                schema: "brokerage",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "BlockingDateTime",
                schema: "brokerage",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "CreationDateTime",
                schema: "brokerage",
                table: "accounts");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "brokerage",
                table: "withdrawals",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "brokerage",
                table: "withdrawals",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "brokerage",
                table: "deposits",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "brokerage",
                table: "deposits",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "brokerage",
                table: "broker_accounts",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "brokerage",
                table: "broker_accounts",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "brokerage",
                table: "broker_account_balances",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "brokerage",
                table: "broker_account_balances",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "brokerage",
                table: "accounts",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "brokerage",
                table: "accounts",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "brokerage",
                table: "withdrawals");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "brokerage",
                table: "withdrawals");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "brokerage",
                table: "deposits");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "brokerage",
                table: "broker_accounts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "brokerage",
                table: "broker_account_balances");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "brokerage",
                table: "broker_account_balances");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "brokerage",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "brokerage",
                table: "accounts");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedDateTime",
                schema: "brokerage",
                table: "withdrawals",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDateTime",
                schema: "brokerage",
                table: "withdrawals",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAt",
                schema: "brokerage",
                table: "deposits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAt",
                schema: "brokerage",
                table: "deposits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ConfirmedAt",
                schema: "brokerage",
                table: "deposits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DetectedAt",
                schema: "brokerage",
                table: "deposits",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FailedAt",
                schema: "brokerage",
                table: "deposits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ActivationDateTime",
                schema: "brokerage",
                table: "broker_accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BlockingDateTime",
                schema: "brokerage",
                table: "broker_accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreationDateTime",
                schema: "brokerage",
                table: "broker_accounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AvailableBalanceUpdatedAt",
                schema: "brokerage",
                table: "broker_account_balances",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OwnedBalanceUpdatedAt",
                schema: "brokerage",
                table: "broker_account_balances",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PendingBalanceUpdatedAt",
                schema: "brokerage",
                table: "broker_account_balances",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReservedBalanceUpdateDatedAt",
                schema: "brokerage",
                table: "broker_account_balances",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ActivationDateTime",
                schema: "brokerage",
                table: "accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BlockingDateTime",
                schema: "brokerage",
                table: "accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreationDateTime",
                schema: "brokerage",
                table: "accounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
