using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Change_Key_To_Unique_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_Email",
                table: "Users");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_Username",
                table: "Users");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Regions_Title",
                table: "Regions");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Permissions_Title",
                table: "Permissions");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Genres_Title",
                table: "Genres");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_Title",
                table: "Regions",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Title",
                table: "Permissions",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genres_Title",
                table: "Genres",
                column: "Title",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Regions_Title",
                table: "Regions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_Title",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Genres_Title",
                table: "Genres");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_Username",
                table: "Users",
                column: "Username");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Regions_Title",
                table: "Regions",
                column: "Title");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Permissions_Title",
                table: "Permissions",
                column: "Title");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Genres_Title",
                table: "Genres",
                column: "Title");
        }
    }
}
