using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TazaOrda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColorToDistrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Districts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Districts");
        }
    }
}
