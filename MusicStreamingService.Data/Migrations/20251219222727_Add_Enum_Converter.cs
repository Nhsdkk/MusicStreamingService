using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Enum_Converter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS playlist_import_processed_trigger ON "PlaylistImportStagingEntries";
                """);
            
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PlaylistImportTasks",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PlaylistImportStagingEntries",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE TRIGGER playlist_import_processed_trigger
                    AFTER UPDATE OF "Status"
                    ON "PlaylistImportStagingEntries"
                    FOR EACH ROW
                EXECUTE FUNCTION increment_processed_count();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS playlist_import_processed_trigger ON "PlaylistImportStagingEntries";
                """);
            
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "PlaylistImportTasks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "PlaylistImportStagingEntries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
            
            migrationBuilder.Sql(
                """
                CREATE OR REPLACE TRIGGER playlist_import_processed_trigger
                    AFTER UPDATE OF "Status"
                    ON "PlaylistImportStagingEntries"
                    FOR EACH ROW
                EXECUTE FUNCTION increment_processed_count();
                """);
        }
    }
}
