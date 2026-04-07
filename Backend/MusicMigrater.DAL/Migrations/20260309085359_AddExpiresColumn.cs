using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicMigrater.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddExpiresColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Expires",
                table: "UserSettings",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Expires",
                table: "UserSettings");
        }
    }
}
