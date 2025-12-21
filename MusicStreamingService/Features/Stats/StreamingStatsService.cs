using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Features.Users;

namespace MusicStreamingService.Features.Stats;

public sealed record TopStreamedArtist
{
    [JsonPropertyName("artistId")]
    public Guid ArtistId { get; init; }

    [JsonPropertyName("artistName")]
    public string ArtistName { get; init; } = null!;

    [JsonPropertyName("totalTimePlayedMs")]
    public long TotalTimePlayedMs { get; init; }
}

public sealed record StreamingDate
{
    [JsonPropertyName("date")]
    public DateOnly Date { get; init; }

    [JsonPropertyName("totalTimePlayedMs")]
    public long TotalTimePlayedMs { get; init; }
}

public sealed record TopStreamedSong
{
    public Guid SongId { get; init; }
    
    public string SongTitle { get; init; } = null!;
    
    public long TotalTimePlayedMs { get; init; }
    
    public Guid AlbumId { get; init; }
    
    public string AlbumTitle { get; init; } = null!;
    
    public string AlbumArtworkFilename { get; init; } = null!;

    public string Artists { get; init; } = null!;
    
    [NotMapped]
    public List<ShortSongArtistDto> ArtistsMapped => JsonSerializer.Deserialize<List<ShortSongArtistDto>>(Artists) ?? new List<ShortSongArtistDto>();
}

public interface IStreamingStatsService
{
    /// <summary>
    /// Get top streamed artists for the user
    /// </summary>
    /// <param name="userId">Id of the user</param>
    /// <param name="limit">Amount of artists</param>
    /// <param name="cancellationToken"></param>
    /// <returns>List of top streamed artists</returns>
    public Task<List<TopStreamedArtist>> GetTopStreamedArtistsAsync(
        Guid userId,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get streaming dates for the user
    /// </summary>
    /// <param name="userId">Id of the user</param>
    /// <param name="cancellationToken"></param>
    /// <returns>List of streaming dates</returns>
    public Task<List<StreamingDate>> GetStreamingDatesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get streaming dates for the user's top artist
    /// </summary>
    /// <param name="userId">Id of the user</param>
    /// <param name="cancellationToken"></param>
    /// <returns>List of streaming dates for top artist</returns>
    public Task<List<StreamingDate>> GetTopArtistStreamingDatesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get top streamed songs for the user
    /// </summary>
    /// <param name="userId">Id of the user</param>
    /// <param name="limit">Amount of songs</param>
    /// <param name="cancellationToken"></param>
    /// <returns>List of top songs</returns>
    public Task<List<TopStreamedSong>> GetTopStreamedSongsAsync(
        Guid userId,
        int limit,
        CancellationToken cancellationToken = default);
}

public sealed class StreamingStatsService : IStreamingStatsService
{
    private readonly MusicStreamingContext _context;

    public StreamingStatsService(MusicStreamingContext context)
    {
        _context = context;
    }

    public Task<List<TopStreamedArtist>> GetTopStreamedArtistsAsync(
        Guid userId,
        int limit,
        CancellationToken cancellationToken = default) =>
        _context.Database.SqlQuery<TopStreamedArtist>($"""
                                                       SELECT SUM("StreamingEvents"."TimePlayedSinceLastRequestMs") as "TotalTimePlayedMs",
                                                              artists."Id"                                         as "ArtistId",
                                                              artists."Username"                                    as "ArtistName"
                                                       FROM "StreamingEvents"
                                                                JOIN "Devices" on "Devices"."Id" = "StreamingEvents"."DeviceId"
                                                                JOIN "Users" as device_owners on device_owners."Id" = "Devices"."OwnerId"
                                                                JOIN "Songs" on "Songs"."Id" = "StreamingEvents"."SongId"
                                                                JOIN "SongArtists" on "SongArtists"."SongId" = "Songs"."Id" and "SongArtists"."MainArtist" = true
                                                                JOIN "Users" as artists on artists."Id" = "SongArtists"."ArtistId"
                                                       WHERE device_owners."Id" = {userId}
                                                             AND date_part('year', now()) = date_part('year', "StreamingEvents"."CreatedAt")
                                                       GROUP BY device_owners."Id", artists."Id", artists."Username"
                                                       ORDER BY "TotalTimePlayedMs" DESC
                                                       LIMIT {limit};
                                                       """)
            .ToListAsync(cancellationToken);

    public Task<List<StreamingDate>> GetStreamingDatesAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _context.Database.SqlQuery<StreamingDate>($"""
                                                   SELECT SUM("StreamingEvents"."TimePlayedSinceLastRequestMs") as "TotalTimePlayedMs",
                                                          "StreamingEvents"."CreatedAt"::date                   as "Date"
                                                   FROM "StreamingEvents"
                                                            JOIN "Devices" on "Devices"."Id" = "StreamingEvents"."DeviceId"
                                                            JOIN "Users" as device_owners on device_owners."Id" = "Devices"."OwnerId"
                                                   WHERE device_owners."Id" = {userId}
                                                     AND date_part('year', now()) = date_part('year', "StreamingEvents"."CreatedAt")
                                                   GROUP BY device_owners."Id", "Date"
                                                   ORDER BY "Date";
                                                   """).ToListAsync(cancellationToken);

    public Task<List<StreamingDate>> GetTopArtistStreamingDatesAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _context.Database.SqlQuery<StreamingDate>($"""
                                                   WITH top_artist AS (SELECT SUM("StreamingEvents"."TimePlayedSinceLastRequestMs") as "TotalTimePlayedMs",
                                                                              artists."Id"                                          as "ArtistId",
                                                                              artists."Username"                                    as "ArtistUsername"
                                                                       FROM "StreamingEvents"
                                                                                JOIN "Devices" on "Devices"."Id" = "StreamingEvents"."DeviceId"
                                                                                JOIN "Users" as device_owners on device_owners."Id" = "Devices"."OwnerId"
                                                                                JOIN "Songs" on "Songs"."Id" = "StreamingEvents"."SongId"
                                                                                JOIN "SongArtists"
                                                                                     on "SongArtists"."SongId" = "Songs"."Id" and "SongArtists"."MainArtist" = true
                                                                                JOIN "Users" as artists on artists."Id" = "SongArtists"."ArtistId"
                                                                       WHERE device_owners."Id" = {userId}
                                                                         AND date_part('year', now()) = date_part('year', "StreamingEvents"."CreatedAt")
                                                                       GROUP BY device_owners."Id", artists."Id", artists."Username"
                                                                       ORDER BY "TotalTimePlayedMs" DESC
                                                                       LIMIT 1)

                                                   SELECT SUM("StreamingEvents"."TimePlayedSinceLastRequestMs") as "TotalTimePlayedMs",
                                                          "StreamingEvents"."CreatedAt"::date                   as "Date"
                                                   FROM "StreamingEvents"
                                                            JOIN "Devices" on "Devices"."Id" = "StreamingEvents"."DeviceId"
                                                            JOIN "Users" as device_owners on device_owners."Id" = "Devices"."OwnerId"
                                                            JOIN "Songs" on "Songs"."Id" = "StreamingEvents"."SongId"
                                                            JOIN "SongArtists" on "SongArtists"."SongId" = "Songs"."Id" and "SongArtists"."MainArtist" = true
                                                            JOIN "Users" as artists on artists."Id" = "SongArtists"."ArtistId"
                                                   WHERE device_owners."Id" = {userId}
                                                     AND artists."Id" = (SELECT "ArtistId" FROM top_artist)
                                                     AND date_part('year', now()) = date_part('year', "StreamingEvents"."CreatedAt")
                                                   GROUP BY device_owners."Id", "Date"
                                                   ORDER BY "Date";
                                                   """).ToListAsync(cancellationToken);

    public Task<List<TopStreamedSong>> GetTopStreamedSongsAsync(
        Guid userId,
        int limit,
        CancellationToken cancellationToken = default) =>
        _context.Database.SqlQuery<TopStreamedSong>($"""
                                                     WITH most_played_songs AS (
                                                         SELECT
                                                             SUM("StreamingEvents"."TimePlayedSinceLastRequestMs") as "TotalTimePlayedMs",
                                                             "Songs"."Id" as "SongId"
                                                         FROM "StreamingEvents"
                                                                  JOIN "Devices" on "Devices"."Id" = "StreamingEvents"."DeviceId"
                                                                  JOIN "Users" as device_owners on device_owners."Id" = "Devices"."OwnerId"
                                                                  JOIN "Songs" on "Songs"."Id" = "StreamingEvents"."SongId"
                                                         WHERE device_owners."Id" = {userId}
                                                           AND date_part('year', now()) = date_part('year', "StreamingEvents"."CreatedAt")
                                                         GROUP BY device_owners."Id", "Songs"."Id"
                                                         ORDER BY "TotalTimePlayedMs" DESC
                                                         LIMIT {limit}
                                                     )

                                                     SELECT 
                                                         most_played_songs."TotalTimePlayedMs",
                                                         "Songs"."Id" as "SongId",
                                                         "Songs"."Title" as "SongTitle",
                                                         "Albums"."Id" as "AlbumId",
                                                         "Albums"."Title" as "AlbumTitle",
                                                         "Albums"."S3ArtworkFilename" as "AlbumArtworkFilename",
                                                         json_agg(json_build_object('id', artists."Id", 'username', artists."Username", 'mainArtist', "SongArtists"."MainArtist")) as "Artists"
                                                     FROM most_played_songs
                                                     JOIN "Songs" on "Songs"."Id" = most_played_songs."SongId"
                                                     JOIN "Albums" on "Albums"."Id" = "Songs"."AlbumId"
                                                     JOIN "SongArtists" on "SongArtists"."SongId" = "Songs"."Id"
                                                     JOIN "Users" as artists on artists."Id" = "SongArtists"."ArtistId"
                                                     group by most_played_songs."TotalTimePlayedMs", "Songs"."Id", "Songs"."Title", "Albums"."Id", "Albums"."Title", "Albums"."S3ArtworkFilename"
                                                     ORDER BY most_played_songs."TotalTimePlayedMs" DESC
                                                     """).ToListAsync(cancellationToken);
}