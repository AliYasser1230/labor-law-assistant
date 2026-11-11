using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaborLawAssistant.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredFileName : Migration
    {
        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoredFileName",
                table: "LawDocuments");
        }
    }
}
