using Microsoft.EntityFrameworkCore.Migrations;

namespace Brokerage.Common.Migrations
{
    public partial class AccountAddedEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Account_RequestId",
                schema: "brokerage",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "RequestId",
                schema: "brokerage",
                table: "accounts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestId",
                schema: "brokerage",
                table: "accounts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_RequestId",
                schema: "brokerage",
                table: "accounts",
                column: "RequestId",
                unique: true);
        }
    }
}
