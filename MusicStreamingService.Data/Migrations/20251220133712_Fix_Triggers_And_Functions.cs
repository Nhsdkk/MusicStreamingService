using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Triggers_And_Functions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                create or replace function increment_processed_count() returns trigger
                    language plpgsql
                as
                $$
                BEGIN
                    IF new."Status" != 'Failed' AND new."Status" != 'Matched' THEN
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
                """);

            migrationBuilder.Sql(
                """
                create or replace procedure match_songs_in_playlist_import_batch(IN batch_id uuid, IN similarity_threshold double precision)
                    language plpgsql
                as
                $$
                begin
                    EXECUTE format('SET LOCAL pg_trgm.similarity_threshold = %s', similarity_threshold);
                
                    WITH unprocessed as (
                        SELECT "Id", "SongTitle", "ArtistName", "AlbumName", "ReleaseDate", "PlaylistId"
                        FROM "PlaylistImportStagingEntries"
                        WHERE "BatchId" = batch_id AND "Status" = 'Pending'
                    ), matches as (
                        SELECT
                            unprocessed."Id",
                            CASE song_match."SimilarityScore" > similarity_threshold WHEN TRUE THEN song_match."SongId" END as "SongId",
                            unprocessed."PlaylistId"
                        FROM unprocessed
                        LEFT JOIN LATERAL (
                            SELECT "Songs"."Id"                                                         as "SongId",
                                   "Songs"."Title"                                                      as "Title",
                                   similarity("Songs"."Title", unprocessed."SongTitle") * 0.3 +
                                   similarity("Albums"."Title", unprocessed."AlbumName") * 0.3 +
                                   similarity("Artists"."Username", unprocessed."ArtistName") * 0.2 +
                                   (1.0 / (1 + abs("Albums"."ReleaseDate" - unprocessed."ReleaseDate"::date))) * 0.1 AS "SimilarityScore"
                            FROM "Songs"
                                     JOIN "Albums" on "Songs"."AlbumId" = "Albums"."Id"
                                     JOIN LATERAL (
                                SELECT "SongArtists"."SongId" as "SongId",
                                       "Users"."Username"     as "Username"
                                FROM "SongArtists"
                                         JOIN "Users" on "Users"."Id" = "SongArtists"."ArtistId"
                                WHERE "SongId" = "Songs"."Id"
                                ORDER BY similarity("Users"."Username", unprocessed."ArtistName") DESC
                                LIMIT 1
                                ) as "Artists" on True
                            WHERE "Songs"."Title" % unprocessed."SongTitle"
                               or "Albums"."Title" % unprocessed."AlbumName"
                               or "Artists"."Username" % unprocessed."ArtistName"
                            ORDER BY "SimilarityScore" DESC
                            LIMIT 1
                            ) song_match on true
                    ), matched as (
                        SELECT *
                        FROM matches
                        WHERE matches."SongId" IS NOT NULL
                    ), inserted as (
                        INSERT INTO "PlaylistSongs"
                            ("PlaylistId", "SongId")
                            SELECT "PlaylistId", "SongId" FROM matched
                            ON CONFLICT DO NOTHING
                            RETURNING "SongId"
                    )
                
                    UPDATE "PlaylistImportStagingEntries"
                    SET "Status" = CASE WHEN matches."SongId" is null THEN 'Failed' ELSE 'Matched' END
                    FROM matches
                    WHERE matches."Id" = "PlaylistImportStagingEntries"."Id";
                end;
                $$;
                """);
        }
    }
}
