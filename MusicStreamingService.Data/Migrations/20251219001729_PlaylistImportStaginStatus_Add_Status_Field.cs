using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class PlaylistImportStaginStatus_Add_Status_Field : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PlaylistImportStagingEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION increment_processed_count()
                    RETURNS TRIGGER
                    LANGUAGE plpgsql
                AS
                $$
                BEGIN
                    IF new."Status" != 'Failed' OR new."Status" != 'Processed' THEN
                        RETURN new;
                    END IF;
                    
                    UPDATE "PlaylistImportTasks"
                    SET "ProcessedEntries" = "PlaylistImportTasks"."ProcessedEntries" + 1
                    WHERE "PlaylistImportTasks"."Id" = new."ImportTaskId";
                    
                    RETURN new;
                END;
                $$;

                CREATE OR REPLACE TRIGGER playlist_import_processed_trigger
                    AFTER UPDATE OF "Status"
                    ON "PlaylistImportStagingEntries"
                    FOR EACH ROW
                EXECUTE FUNCTION increment_processed_count();
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "PlaylistImportStagingEntries");

            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS playlist_import_processed_trigger ON "PlaylistImportStagingEntries";
                """);
        }
    }
}
