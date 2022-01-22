using Microsoft.EntityFrameworkCore.Migrations;

namespace DFPay.MVC.Data.Migrations
{
    public partial class CleanUp_AspNetNavigationMenu_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "AspNetNavigationMenu");

            migrationBuilder.DropColumn(
                name: "ExternalUrl",
                table: "AspNetNavigationMenu");

            migrationBuilder.DropColumn(
                name: "IsExternal",
                table: "AspNetNavigationMenu");

            migrationBuilder.DropColumn(
                name: "Visible",
                table: "AspNetNavigationMenu");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "AspNetNavigationMenu",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalUrl",
                table: "AspNetNavigationMenu",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternal",
                table: "AspNetNavigationMenu",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Visible",
                table: "AspNetNavigationMenu",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
