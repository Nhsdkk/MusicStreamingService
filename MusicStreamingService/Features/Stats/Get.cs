using System.Text.Json.Serialization;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStreamingService.Extensions;
using MusicStreamingService.Features.Users;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Stats;

[ApiController]
public class Get : ControllerBase
{
    private readonly IMediator _mediator;

    public Get(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get streaming statistics for the authenticated user.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/stats")]
    [Tags(RouteGroups.Stats)]
    [Authorize(Roles = Permissions.PlaySongsPermission)]
    [ProducesResponseType<QueryResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Handle(
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Query
            {
                UserId = User.GetUserId()
            }, cancellationToken);

        return Ok(result);
    }

    public sealed record Query : IRequest<QueryResponse>
    {
        public Guid UserId { get; init; }
    }

    public sealed record QueryResponse
    {
        public sealed record ArtistStatsDto
        {
            [JsonPropertyName("id")]
            public Guid Id { get; init; }

            [JsonPropertyName("username")]
            public string Username { get; init; } = null!;

            [JsonPropertyName("totalStreamedTimeMs")]
            public long TotalStreamedTimeMs { get; init; }

            public static ArtistStatsDto FromEntity(
                TopStreamedArtist artist) => new ArtistStatsDto
            {
                Id = artist.ArtistId,
                Username = artist.ArtistName,
                TotalStreamedTimeMs = artist.TotalTimePlayedMs
            };
        }

        public sealed record StreamingDateDto
        {
            [JsonPropertyName("date")]
            public DateOnly Date { get; init; }

            [JsonPropertyName("totalTimePlayedMs")]
            public long TotalTimePlayedMs { get; init; }

            public static StreamingDateDto FromEntity(
                StreamingDate streamingDate) => new StreamingDateDto
            {
                Date = streamingDate.Date,
                TotalTimePlayedMs = streamingDate.TotalTimePlayedMs
            };
        }

        public sealed record TopStreamedSongDto
        {
            [JsonPropertyName("songId")]
            public Guid SongId { get; init; }

            [JsonPropertyName("songTitle")]
            public string SongTitle { get; init; } = null!;

            [JsonPropertyName("totalTimePlayedMs")]
            public long TotalTimePlayedMs { get; init; }

            [JsonPropertyName("albumId")]
            public Guid AlbumId { get; init; }

            [JsonPropertyName("albumTitle")]
            public string AlbumTitle { get; init; } = null!;

            [JsonPropertyName("albumArtworkUrl")]
            public string? AlbumArtworkUrl { get; init; } = null!;

            [JsonPropertyName("artists")]
            public List<ShortSongArtistDto> Artists { get; init; } = null!;

            public static TopStreamedSongDto FromEntity(
                TopStreamedSong topStreamedSong,
                string? albumArtworkUrl) => new TopStreamedSongDto
            {
                SongId = topStreamedSong.SongId,
                SongTitle = topStreamedSong.SongTitle,
                TotalTimePlayedMs = topStreamedSong.TotalTimePlayedMs,
                AlbumId = topStreamedSong.AlbumId,
                AlbumTitle = topStreamedSong.AlbumTitle,
                AlbumArtworkUrl = albumArtworkUrl,
                Artists = topStreamedSong.ArtistsMapped
            };
        }

        [JsonPropertyName("topArtists")]
        public List<ArtistStatsDto> TopArtists { get; init; } = null!;

        [JsonPropertyName("topSongs")]
        public List<TopStreamedSongDto> TopStreamedSongs { get; init; } = null!;

        [JsonPropertyName("streamingDates")]
        public List<StreamingDateDto> StreamingDates { get; init; } = null!;

        [JsonPropertyName("streamingDatesByTopArtist")]
        public List<StreamingDateDto> StreamingDatesByTopArtist { get; init; } = null!;

        [JsonPropertyName("totalStreamedTimeMs")]
        public long TotalStreamedTimeMs { get; init; }

        public static QueryResponse FromStats(
            List<TopStreamedArtist> topArtists,
            List<TopStreamedSong> topSongs,
            List<StreamingDate> streamingDates,
            List<StreamingDate> streamingDatesByTopArtist,
            Dictionary<string, string?> albumArtworkMapping) => new QueryResponse
        {
            TopArtists = topArtists.Select(ArtistStatsDto.FromEntity).ToList(),
            TopStreamedSongs = topSongs
                .Select(x => TopStreamedSongDto.FromEntity(
                    x,
                    albumArtworkMapping[x.AlbumArtworkFilename]))
                .ToList(),
            StreamingDates = streamingDates.Select(StreamingDateDto.FromEntity).ToList(),
            StreamingDatesByTopArtist = streamingDatesByTopArtist.Select(StreamingDateDto.FromEntity).ToList(),
            TotalStreamedTimeMs = streamingDates.Sum(x => x.TotalTimePlayedMs),
        };
    }
    
    public sealed class Handler : IRequestHandler<Query, QueryResponse>
    {
        private readonly IStreamingStatsService _streamingStatsService;
        private readonly IAlbumStorageService _albumStorageService;

        public Handler(
            IStreamingStatsService streamingStatsService,
            IAlbumStorageService albumStorageService)
        {
            _streamingStatsService = streamingStatsService;
            _albumStorageService = albumStorageService;
        }

        public async ValueTask<QueryResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var topSongs = await _streamingStatsService.GetTopStreamedSongsAsync( 
                request.UserId,
                5,
                cancellationToken);
            var topArtists = await _streamingStatsService.GetTopStreamedArtistsAsync(
                request.UserId,
                5,
                cancellationToken);
            var streamingDates = await _streamingStatsService.GetStreamingDatesAsync(
                request.UserId,
                cancellationToken);
            var streamingDatesByTopArtist = await _streamingStatsService.GetTopArtistStreamingDatesAsync(
                request.UserId,
                cancellationToken);

            var albumArtworkFilenames = topSongs.Select(x => x.AlbumArtworkFilename).ToList();
            var albumArtworkUrls = await _albumStorageService.GetPresignedUrls(
                albumArtworkFilenames,
                cancellationToken);

            return QueryResponse.FromStats(
                topArtists, 
                topSongs, 
                streamingDates, 
                streamingDatesByTopArtist,
                albumArtworkUrls);
        }
    }
}