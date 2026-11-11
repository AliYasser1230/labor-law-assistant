using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaborLawAssistant.Migrations
{
    public partial class AddStoredFileName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoredFileName",
                table: "LawDocuments",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoredFileName",
                table: "LawDocuments");
        }
    }
}
