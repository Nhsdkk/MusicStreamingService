using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_liked_by_users_to_albums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_Users_UserEntityId",
                table: "Albums");

            migrationBuilder.DropIndex(
                name: "IX_Albums_UserEntityId",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "UserEntityId",
                table: "Albums");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserEntityId",
                table: "Albums",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_UserEntityId",
                table: "Albums",
                column: "UserEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_Users_UserEntityId",
                table: "Albums",
                column: "UserEntityId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
