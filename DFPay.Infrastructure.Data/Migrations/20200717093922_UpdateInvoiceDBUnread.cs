using Microsoft.EntityFrameworkCore.Migrations;

namespace DFPay.Infrastructure.Data.Migrations
{
    public partial class UpdateInvoiceDBUnread : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Unread",
                table: "Invoices",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unread",
                table: "Invoices");
        }
    }
}
