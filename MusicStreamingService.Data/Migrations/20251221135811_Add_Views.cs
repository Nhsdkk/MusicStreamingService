using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Views : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 create or replace view "v_artist_metrics" as
                                 with artist_songs as (
                                     select sa."ArtistId", s."Id" as "SongId"
                                     from "SongArtists" sa
                                              join "Songs" s on s."Id" = sa."SongId"
                                 ),
                                      artist_streams as (
                                          select
                                              sa."ArtistId",
                                              count(se."Id") as stream_events,
                                              coalesce(sum(se."TimePlayedSinceLastRequestMs"), 0) as total_played_ms,
                                              max(se."CreatedAt") as last_stream_at
                                          from artist_songs sa
                                                   join "StreamingEvents" se on se."SongId" = sa."SongId"
                                          group by sa."ArtistId"
                                      ),
                                      artist_playlist_adds as (
                                          select
                                              sa."ArtistId",
                                              count(ps."SongId") as playlist_adds
                                          from artist_songs sa
                                                   join "PlaylistSongs" ps on ps."SongId" = sa."SongId"
                                          group by sa."ArtistId"
                                      )
                                 select
                                     u."Id"                               as "ArtistId",
                                     u."Username"                         as "ArtistUsername",
                                     u."FullName",
                                     u."RegionId",
                                     count(distinct al."Id")              as "AlbumCount",
                                     count(distinct sa."SongId")          as "SongCount",
                                     coalesce(sum(s."Likes"), 0)          as "SongLikes",
                                     coalesce(sum(al."Likes"), 0)         as "AlbumLikes",
                                     coalesce(pa.playlist_adds, 0)        as "PlaylistAdds",
                                     coalesce(st.stream_events, 0)        as "StreamEvents",
                                     coalesce(st.total_played_ms, 0)      as "TotalPlayedMs",
                                     st.last_stream_at                    as "LastStreamAt"
                                 from "Users" u
                                          left join "Albums" al on al."ArtistId" = u."Id"
                                          left join "SongArtists" sa on sa."ArtistId" = u."Id"
                                          left join "Songs" s on s."Id" = sa."SongId"
                                          left join artist_streams st on st."ArtistId" = u."Id"
                                          left join artist_playlist_adds pa on pa."ArtistId" = u."Id"
                                 group by u."Id", u."Username", u."FullName", u."RegionId", st.stream_events, st.total_played_ms, st.last_stream_at, pa.playlist_adds;
                                 
                                 create or replace view "v_region_overview" as
                                 with region_streams as (
                                     select
                                         r."Id" as "RegionId",
                                         count(se."Id") as stream_events,
                                         coalesce(sum(se."TimePlayedSinceLastRequestMs"), 0) as total_played_ms
                                     from "Regions" r
                                              join "Users" u on u."RegionId" = r."Id"
                                              join "Devices" d on d."OwnerId" = u."Id"
                                              join "StreamingEvents" se on se."DeviceId" = d."Id"
                                     group by r."Id"
                                 ),
                                      region_available_songs as (
                                          select
                                              ad."RegionId",
                                              count(distinct ad."SongId") as available_songs
                                          from "AllowedDistribution" ad
                                          group by ad."RegionId"
                                      )
                                 select
                                     r."Id"                          as "RegionId",
                                     r."Title"                       as "RegionTitle",
                                     count(distinct u."Id")          as "UserCount",
                                     count(distinct ar."Id")         as "ArtistCount",
                                     count(distinct pl."Id")         as "PlaylistCount",
                                     count(distinct d."Id")          as "DeviceCount",
                                     coalesce(ra.available_songs,0)  as "AvailableSongs",
                                     coalesce(rs.stream_events,0)    as "StreamEvents",
                                     coalesce(rs.total_played_ms,0)  as "TotalPlayedMs"
                                 from "Regions" r
                                          left join "Users" u on u."RegionId" = r."Id"
                                          left join "Users" ar on ar."RegionId" = r."Id" and exists (
                                     select 1 from "Albums" al where al."ArtistId" = ar."Id")
                                          left join "Playlists" pl on pl."CreatorId" = u."Id"
                                          left join "Devices" d on d."OwnerId" = u."Id"
                                          left join region_streams rs on rs."RegionId" = r."Id"
                                          left join region_available_songs ra on ra."RegionId" = r."Id"
                                 group by r."Id", r."Title", ra.available_songs, rs.stream_events, rs.total_played_ms;
                                 
                                 create or replace view "v_genre_performance" as
                                 with genre_streams as (
                                     select
                                         sg."GenreId",
                                         count(se."Id") as stream_events,
                                         coalesce(sum(se."TimePlayedSinceLastRequestMs"), 0) as total_played_ms
                                     from "SongGenres" sg
                                              join "StreamingEvents" se on se."SongId" = sg."SongId"
                                     group by sg."GenreId"
                                 )
                                 select
                                     g."Id"                        as "GenreId",
                                     g."Title"                     as "GenreTitle",
                                     g."Description",
                                     count(distinct sg."SongId")   as "SongCount",
                                     count(distinct sa."ArtistId") as "ArtistCount",
                                     coalesce(gs.stream_events,0)  as "StreamEvents",
                                     coalesce(gs.total_played_ms,0) as "TotalPlayedMs"
                                 from "Genres" g
                                          left join "SongGenres" sg on sg."GenreId" = g."Id"
                                          left join "SongArtists" sa on sa."SongId" = sg."SongId"
                                          left join genre_streams gs on gs."GenreId" = g."Id"
                                 group by g."Id", g."Title", g."Description", gs.stream_events, gs.total_played_ms;
                                 
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop view if exists \"v_artist_metrics\";");
            migrationBuilder.Sql("drop view if exists \"v_region_overview\";");
            migrationBuilder.Sql("drop view if exists \"v_genre_performance\";");
        }
    }
}
