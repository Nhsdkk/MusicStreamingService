using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class PlaylistSongEntity_Change_Position_To_DateAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "PlaylistSongs");

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedAt",
                table: "PlaylistSongs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedAt",
                table: "PlaylistSongs");

            migrationBuilder.AddColumn<long>(
                name: "Position",
                table: "PlaylistSongs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
