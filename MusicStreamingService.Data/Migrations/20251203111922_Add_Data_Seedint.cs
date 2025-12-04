using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Data_Seedint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
               migrationBuilder.Sql(
                      """
                      INSERT INTO "Permissions" ("Title", "Description")
                      VALUES ('mss.tracks.view', ''),
                             ('mss.tracks.manage', ''),
                             ('mss.tracks.playback', ''),
                             ('mss.tracks.favorite', ''),
                             ('mss.tracks.admin', ''),
                             ('mss.albums.view', ''),
                             ('mss.albums.favorite', ''),
                             ('mss.albums.manage', ''),
                             ('mss.albums.admin', ''),
                             ('mss.users.view', ''),
                             ('mss.users.manage', ''),
                             ('mss.users.admin', ''),
                             ('mss.subscriptions.view', ''),
                             ('mss.subscriptions.manage', ''),
                             ('mss.subscriptions.admin', ''),
                             ('mss.playlists.view', ''),
                             ('mss.playlists.favorite', ''),
                             ('mss.playlists.manage', ''),
                             ('mss.playlists.admin', ''),
                             ('mss.genres.view', ''),
                             ('mss.genres.admin', ''),
                             ('mss.regions.view', ''),
                             ('mss.regions.admin', ''),
                             ('mss.streaming-events.view', ''),
                             ('mss.streaming-events.manage', ''),
                             ('mss.streaming-events.admin', ''),
                             ('mss.payments.view', ''),
                             ('mss.payments.manage', ''),
                             ('mss.payments.admin', '');

                      INSERT INTO "Roles" ("Title", "Description")
                      VALUES ('mss.user', ''),
                             ('mss.artist', ''),
                             ('mss.admin', '');

                      with admin_permissions as (Select "Id", "Title"
                                                 FROM "Permissions"),
                           artist_permissions as (Select "Id", "Title"
                                                  FROM admin_permissions
                                                  WHERE "Title" NOT LIKE '%admin'),
                           user_permissions as (Select "Id", "Title"
                                                FROM artist_permissions
                                                WHERE "Title" NOT LIKE '%tracks.manage'
                                                  AND "Title" NOT LIKE '%albums.manage')

                      INSERT INTO "RolePermissions" ("RoleId", "PermissionId")
                      SELECT (SELECT "Id" FROM "Roles" WHERE "Title" = 'mss.admin') AS "RoleId",
                             ap."Id" AS "PermissionId"
                      FROM admin_permissions ap

                      UNION ALL

                      SELECT (SELECT "Id" FROM "Roles" WHERE "Title" = 'mss.artist') AS "RoleId",
                             art."Id" AS "PermissionId"
                      FROM artist_permissions art

                      UNION ALL

                      SELECT (SELECT "Id" FROM "Roles" WHERE "Title" = 'mss.user') AS "RoleId",
                             up."Id" AS "PermissionId"
                      FROM user_permissions up;
                      """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
               migrationBuilder.Sql(
                      """
                      TRUNCATE "Permissions", "Roles" CASCADE;
                      """);
        }
    }
}
