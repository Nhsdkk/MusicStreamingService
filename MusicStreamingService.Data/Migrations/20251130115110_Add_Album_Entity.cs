using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Album_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Likes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ArtistId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    S3ArtworkFilename = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Albums_Users_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumFavorites",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlbumId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumFavorites", x => new { x.AlbumId, x.UserId });
                    table.ForeignKey(
                        name: "FK_AlbumFavorites_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumFavorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumFavorites_UserId",
                table: "AlbumFavorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ArtistId",
                table: "Albums",
                column: "ArtistId");

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION increment_likes()
                    RETURNS TRIGGER
                    LANGUAGE plpgsql
                AS
                $$
                BEGIN
                    UPDATE "Albums"
                    SET "Likes" = "Albums"."Likes" + 1
                    WHERE "Albums"."Id" = new."AlbumId";
                    RETURN new;
                END;
                $$;
                
                CREATE OR REPLACE TRIGGER album_favorite 
                AFTER INSERT
                ON "AlbumFavorites"
                FOR EACH ROW
                EXECUTE FUNCTION increment_likes();
                """);
            
            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION decrement_likes()
                    RETURNS TRIGGER
                    LANGUAGE plpgsql
                AS
                $$
                BEGIN
                    UPDATE "Albums"
                    SET "Likes" = "Albums"."Likes" - 1
                    WHERE "Albums"."Id" = old."AlbumId";
                    RETURN new;
                END;
                $$;
                
                CREATE OR REPLACE TRIGGER album_unfavorite 
                AFTER DELETE
                ON "AlbumFavorites"
                FOR EACH ROW
                EXECUTE FUNCTION decrement_likes();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER album_unfavorite ON \"AlbumFavorites\"; DROP FUNCTION decrement_likes;");
            migrationBuilder.Sql("DROP TRIGGER album_favorite ON \"AlbumFavorites\"; DROP FUNCTION increment_likes;");
            
            migrationBuilder.DropTable(
                name: "AlbumFavorites");

            migrationBuilder.DropTable(
                name: "Albums");
        }
    }
}
