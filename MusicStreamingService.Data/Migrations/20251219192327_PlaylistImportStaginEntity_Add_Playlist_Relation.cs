using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class PlaylistImportStaginEntity_Add_Playlist_Relation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlaylistId",
                table: "PlaylistImportStagingEntries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistImportStagingEntries_PlaylistId",
                table: "PlaylistImportStagingEntries",
                column: "PlaylistId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistImportStagingEntries_Playlists_PlaylistId",
                table: "PlaylistImportStagingEntries",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE PROCEDURE match_songs_in_playlist_import_batch(batch_id UUID, similarity_threshold double precision)
                language plpgsql
                AS
                $$
                begin
                    WITH unprocessed as (
                        SELECT "Id", "SongTitle", "ArtistName", "AlbumName", "ReleaseDate", "PlaylistId"
                        FROM "PlaylistImportStagingEntries"
                        WHERE "BatchId" = batch_id AND "Status" = 'Pending'
                    ), matches as (
                        SELECT
                            unprocessed."Id",
                            song_match."SongId",
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
                            WHERE "Songs"."Title" ILIKE '%' || unprocessed."SongTitle" || '%'
                                AND "Albums"."Title" ILIKE '%' || unprocessed."AlbumName" || '%'
                                AND "Artists"."Username" ILIKE '%' || unprocessed."ArtistName" || '%'
                                AND "Albums"."ReleaseDate" BETWEEN unprocessed."ReleaseDate" - '1 year'::interval AND unprocessed."ReleaseDate" + '1 year'::interval
                            ORDER BY "SimilarityScore" DESC
                            LIMIT 1
                            ) song_match on true
                        WHERE "SimilarityScore" > similarity_threshold
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
            
            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS match_songs_in_playlist_import_batch(UUID, UUID, double precision);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistImportStagingEntries_Playlists_PlaylistId",
                table: "PlaylistImportStagingEntries");

            migrationBuilder.DropIndex(
                name: "IX_PlaylistImportStagingEntries_PlaylistId",
                table: "PlaylistImportStagingEntries");

            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "PlaylistImportStagingEntries");

            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS match_songs_in_playlist_import_batch(UUID, double precision);");
            
            migrationBuilder.Sql(
                """
                CREATE OR REPLACE PROCEDURE match_songs_in_playlist_import_batch(batch_id UUID, playlist_id UUID, similarity_threshold double precision)
                language plpgsql
                AS
                $$
                begin
                    WITH unprocessed as (
                        SELECT "Id", "SongTitle", "ArtistName", "AlbumName", "ReleaseDate"
                        FROM "PlaylistImportStagingEntries"
                        WHERE "BatchId" = batch_id AND "Status" = 'Pending'
                    ), matches as (
                        SELECT
                            unprocessed."Id",
                            song_match."SongId"
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
                            WHERE "Songs"."Title" ILIKE '%' || unprocessed."SongTitle" || '%'
                                AND "Albums"."Title" ILIKE '%' || unprocessed."AlbumName" || '%'
                                AND "Artists"."Username" ILIKE '%' || unprocessed."ArtistName" || '%'
                                AND "Albums"."ReleaseDate" BETWEEN unprocessed."ReleaseDate" - '1 year'::interval AND unprocessed."ReleaseDate" + '1 year'::interval
                            ORDER BY "SimilarityScore" DESC
                            LIMIT 1
                            ) song_match on true
                        WHERE "SimilarityScore" > similarity_threshold
                    ), matched as (
                        SELECT *
                        FROM matches
                        WHERE matches."SongId" IS NOT NULL
                    ), inserted as (
                        INSERT INTO "PlaylistSongs"
                        ("PlaylistId", "SongId")
                        SELECT playlist_id, "SongId" FROM matched   
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
