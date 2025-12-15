using System.Text.Json.Serialization;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Extensions;
using MusicStreamingService.Features.Albums;
using MusicStreamingService.Features.Devices;
using MusicStreamingService.Features.Genres;
using MusicStreamingService.Features.Region;
using MusicStreamingService.Features.Users;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.StreamingEvents;

[ApiController]
public sealed class GetLatest : ControllerBase
{
    private readonly IMediator _mediator;

    public GetLatest(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get latest streaming event for a device with song data
    /// </summary>
    /// <param name="request">Id of the device</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/streaming-events/latest")]
    [Tags(RouteGroups.StreamingEvents)]
    [Authorize(Roles = Permissions.PlaySongsPermission)]
    [ProducesResponseType<QueryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLatestStreamingEvent(
        [FromQuery] Query.QueryBody request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Query
            {
                Body = request,
                UserId = User.GetUserId()
            }, cancellationToken);

        return result.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record Query : IRequest<Result<QueryResponse>>
    {
        public sealed record QueryBody
        {
            [JsonPropertyName("deviceId")]
            public Guid DeviceId { get; init; }
        }

        public QueryBody Body { get; init; } = null!;

        public Guid UserId { get; init; }
    }

    public sealed record QueryResponse
    {
        public sealed record LastPlayedSong
        {
            [JsonPropertyName("id")]
            public Guid Id { get; init; }

            [JsonPropertyName("title")]
            public string Title { get; init; } = null!;

            [JsonPropertyName("durationMs")]
            public long DurationMs { get; init; }

            [JsonPropertyName("songUrl")]
            public string? SongUrl { get; init; }

            [JsonPropertyName("likes")]
            public long Likes { get; init; }

            [JsonPropertyName("explicit")]
            public bool Explicit { get; init; }

            [JsonPropertyName("artists")]
            public List<ShortSongArtistDto> Artists { get; init; } = new();

            [JsonPropertyName("allowedRegions")]
            public List<ShortRegionDto> AllowedRegions { get; init; } = new();

            [JsonPropertyName("album")]
            public ShortAlbumDto Album { get; init; } = new();

            [JsonPropertyName("genres")]
            public List<ShortGenreDto> Genres { get; init; } = new();

            public static LastPlayedSong FromEntity(
                SongEntity song,
                string? songUrl,
                string? albumArtUrl) =>
                new LastPlayedSong
                {
                    Id = song.Id,
                    Title = song.Title,
                    DurationMs = song.DurationMs,
                    SongUrl = songUrl,
                    Likes = song.Likes,
                    Explicit = song.Explicit,
                    Artists = song.Artists
                        .Select(x => ShortSongArtistDto.FromEntity(x.Artist, x.MainArtist))
                        .ToList(),
                    AllowedRegions = song.AllowedRegions
                        .Select(ShortRegionDto.FromEntity)
                        .ToList(),
                    Album = ShortAlbumDto.FromEntity(song.Album, albumArtUrl),
                    Genres = song.Genres
                        .Select(ShortGenreDto.FromEntity)
                        .ToList()
                };
        }

        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("device")]
        public DeviceDto Device { get; init; } = null!;

        [JsonPropertyName("positionMs")]
        public long PositionMs { get; init; }

        [JsonPropertyName("song")]
        public LastPlayedSong Song { get; init; } = null!;

        [JsonPropertyName("eventType")]
        public StreamingEventType EventType { get; init; }

        public static QueryResponse FromEntity(
            StreamingEventEntity streamingEvent,
            string songUrl,
            string artworkUrl) =>
            new QueryResponse
            {
                Id = streamingEvent.Id,
                Device = new DeviceDto
                {
                    Id = streamingEvent.Device.Id,
                    Title = streamingEvent.Device.Title
                },
                PositionMs = streamingEvent.PositionMs,
                Song = LastPlayedSong.FromEntity(
                    streamingEvent.Song,
                    songUrl,
                    artworkUrl),
                EventType = streamingEvent.EventType
            };
    }

    public sealed class Handler : IRequestHandler<Query, Result<QueryResponse>>
    {
        private readonly MusicStreamingContext _context;
        private readonly IAlbumStorageService _albumStorageService;
        private readonly ISongStorageService _songStorageService;

        public Handler(
            MusicStreamingContext context,
            ISongStorageService songStorageService,
            IAlbumStorageService albumStorageService)
        {
            _context = context;
            _songStorageService = songStorageService;
            _albumStorageService = albumStorageService;
        }

        public async ValueTask<Result<QueryResponse>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var lastEvent = await _context.StreamingEvents
                .AsNoTracking()
                .Include(x => x.Device)
                .Include(x => x.Song)
                .ThenInclude(x => x.Artists)
                .ThenInclude(x => x.Artist)
                .Include(x => x.Song)
                .ThenInclude(x => x.AllowedRegions)
                .Include(x => x.Song)
                .ThenInclude(x => x.Genres)
                .Include(x => x.Song)
                .ThenInclude(x => x.Album)
                .Where(x => x.DeviceId == request.Body.DeviceId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastEvent is null)
            {
                return new Exception("No streaming events found for this device");
            }

            if (lastEvent.Device.OwnerId != request.UserId)
            {
                return new Exception("Device does not belong to the user");
            }

            var songUrlResult = await _songStorageService.GetPresignedUrl(lastEvent.Song.S3MediaFileName);
            if (songUrlResult.IsError)
            {
                return songUrlResult.Error();
            }

            var albumArtUrlResult = await _albumStorageService.GetPresignedUrl(lastEvent.Song.Album.S3ArtworkFilename);
            if (albumArtUrlResult.IsError)
            {
                return albumArtUrlResult.Error();
            }

            return QueryResponse.FromEntity(lastEvent, songUrlResult.Success(), albumArtUrlResult.Success());
        }
    }
}