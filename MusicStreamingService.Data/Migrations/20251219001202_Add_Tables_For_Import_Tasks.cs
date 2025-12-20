using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Tables_For_Import_Tasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlaylistImportTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    S3FileName = table.Column<string>(type: "text", nullable: false),
                    TotalEntries = table.Column<long>(type: "bigint", nullable: false),
                    ProcessedEntries = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistImportTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaylistImportTasks_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistImportStagingEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    SongTitle = table.Column<string>(type: "text", nullable: false),
                    AlbumName = table.Column<string>(type: "text", nullable: false),
                    ArtistName = table.Column<string>(type: "text", nullable: false),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistImportStagingEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaylistImportStagingEntries_PlaylistImportTasks_ImportTask~",
                        column: x => x.ImportTaskId,
                        principalTable: "PlaylistImportTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistImportStagingEntries_ImportTaskId",
                table: "PlaylistImportStagingEntries",
                column: "ImportTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistImportTasks_CreatorId",
                table: "PlaylistImportTasks",
                column: "CreatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaylistImportStagingEntries");

            migrationBuilder.DropTable(
                name: "PlaylistImportTasks");
        }
    }
}
