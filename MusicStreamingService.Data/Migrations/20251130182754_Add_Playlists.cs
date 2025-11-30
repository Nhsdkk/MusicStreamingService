using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Playlists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessType = table.Column<string>(type: "text", nullable: false),
                    Likes = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    UserEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlists_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Playlists_Users_UserEntityId",
                        column: x => x.UserEntityId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlaylistFavorites",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaylistId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistFavorites", x => new { x.PlaylistId, x.UserId });
                    table.ForeignKey(
                        name: "FK_PlaylistFavorites_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistFavorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistSongs",
                columns: table => new
                {
                    PlaylistId = table.Column<Guid>(type: "uuid", nullable: false),
                    SongId = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistSongs", x => new { x.SongId, x.PlaylistId });
                    table.ForeignKey(
                        name: "FK_PlaylistSongs_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistSongs_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistFavorites_UserId",
                table: "PlaylistFavorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_CreatorId",
                table: "Playlists",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_UserEntityId",
                table: "Playlists",
                column: "UserEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistSongs_PlaylistId",
                table: "PlaylistSongs",
                column: "PlaylistId");
            
            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION increment_likes_playlist()
                    RETURNS TRIGGER
                    LANGUAGE plpgsql
                AS
                $$
                BEGIN
                    UPDATE "Playlists"
                    SET "Likes" = "Playlists"."Likes" + 1
                    WHERE "Playlists"."Id" = new."PlaylistId";
                    RETURN new;
                END;
                $$;

                CREATE OR REPLACE TRIGGER playlist_favorite 
                AFTER INSERT
                ON "PlaylistFavorites"
                FOR EACH ROW
                EXECUTE FUNCTION increment_likes_playlist();
                """);
            
            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION decrement_likes_playlist()
                    RETURNS TRIGGER
                    LANGUAGE plpgsql
                AS
                $$
                BEGIN
                    UPDATE "Playlists"
                    SET "Likes" = "Playlists"."Likes" - 1
                    WHERE "Playlists"."Id" = old."PlaylistId";
                    RETURN new;
                END;
                $$;

                CREATE OR REPLACE TRIGGER playlist_unfavorite 
                AFTER DELETE
                ON "PlaylistFavorites"
                FOR EACH ROW
                EXECUTE FUNCTION decrement_likes_playlist();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER playlist_unfavorite ON \"PlaylistFavorites\"; DROP FUNCTION decrement_likes_playlist;");
            migrationBuilder.Sql("DROP TRIGGER playlist_favorite ON \"PlaylistFavorites\"; DROP FUNCTION increment_likes_playlist;");
            
            migrationBuilder.DropTable(
                name: "PlaylistFavorites");

            migrationBuilder.DropTable(
                name: "PlaylistSongs");

            migrationBuilder.DropTable(
                name: "Playlists");
        }
    }
}
