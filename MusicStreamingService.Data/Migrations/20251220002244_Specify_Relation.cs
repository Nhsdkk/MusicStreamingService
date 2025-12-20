using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Specify_Relation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Defensive no-op if a previous migration already removed this relation.
            migrationBuilder.Sql("""
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM information_schema.table_constraints
                    WHERE constraint_name = 'FK_Playlists_Users_UserEntityId'
                      AND table_name = 'Playlists'
                ) THEN
                    ALTER TABLE "Playlists" DROP CONSTRAINT "FK_Playlists_Users_UserEntityId";
                END IF;

                IF EXISTS (
                    SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_Playlists_UserEntityId'
                ) THEN
                    DROP INDEX "IX_Playlists_UserEntityId";
                END IF;

                IF EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_name = 'Playlists'
                      AND column_name = 'UserEntityId'
                ) THEN
                    ALTER TABLE "Playlists" DROP COLUMN "UserEntityId";
                END IF;
            END
            $$;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserEntityId",
                table: "Playlists",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_UserEntityId",
                table: "Playlists",
                column: "UserEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_Users_UserEntityId",
                table: "Playlists",
                column: "UserEntityId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
