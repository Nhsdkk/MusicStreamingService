using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Navigation_Properties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_Users_UserEntityId",
                table: "Playlists");

            migrationBuilder.DropIndex(
                name: "IX_Playlists_UserEntityId",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "UserEntityId",
                table: "Playlists");
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
