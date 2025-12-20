using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Indexes_For_Song_Matching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Songs_Title",
                table: "Songs",
                column: "Title")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ReleaseDate",
                table: "Albums",
                column: "ReleaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Title",
                table: "Albums",
                column: "Title")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Songs_Title",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Albums_ReleaseDate",
                table: "Albums");

            migrationBuilder.DropIndex(
                name: "IX_Albums_Title",
                table: "Albums");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }
    }
}
