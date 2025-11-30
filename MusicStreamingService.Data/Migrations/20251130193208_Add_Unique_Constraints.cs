using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Unique_Constraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Regions_Title",
                table: "Regions",
                column: "Title");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Genres_Title",
                table: "Genres",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Regions_Title",
                table: "Regions");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Genres_Title",
                table: "Genres");
        }
    }
}
