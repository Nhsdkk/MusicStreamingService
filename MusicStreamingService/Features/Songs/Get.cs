using System.Text.Json.Serialization;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Extensions;
using MusicStreamingService.Features.Albums;
using MusicStreamingService.Features.Genres;
using MusicStreamingService.Features.Region;
using MusicStreamingService.Features.Users;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Songs;

[ApiController]
public sealed class Get : ControllerBase
{
    private readonly IMediator _mediator;

    public Get(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get song data
    /// </summary>
    /// <param name="songId">Id of the song</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/songs/{songId}")]
    [Tags(RouteGroups.Songs)]
    [Authorize(Roles = Permissions.ViewSongsPermission)]
    [ProducesResponseType<QueryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSong(
        [FromRoute] Guid songId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Query
            {
                SongId = songId,
                UserRegion = User.GetUserRegion(),
                UserAge = User.GetUserAge(),
            },
            cancellationToken);

        return result.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record Query : IRequest<Result<QueryResponse>>
    {
        public Guid SongId { get; init; }

        public RegionClaim UserRegion { get; init; } = null!;
        
        public int UserAge { get; init; }
    }

    public sealed record QueryResponse
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

        [JsonPropertyName("allowedInUserRegion")]
        public bool AllowedInUserRegion { get; init; }

        [JsonPropertyName("album")]
        public ShortAlbumDto Album { get; init; } = new();

        [JsonPropertyName("genres")]
        public List<ShortGenreDto> Genres { get; init; } = new();

        public static QueryResponse FromEntity(
            SongEntity song,
            string? songUrl,
            string? albumArtUrl,
            RegionClaim userRegion) =>
            new QueryResponse
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
                AllowedInUserRegion = song.AllowedRegions.Any(r => r.Id == userRegion.Id),
                Album = ShortAlbumDto.FromEntity(song.Album, albumArtUrl),
                Genres = song.Genres
                    .Select(ShortGenreDto.FromEntity)
                    .ToList()
            };
    }

    public sealed class Handler : IRequestHandler<Query, Result<QueryResponse>>
    {
        private readonly MusicStreamingContext _context;
        private readonly ISongStorageService _songStorageService;
        private readonly IAlbumStorageService _albumStorageService;

        public Handler(
            MusicStreamingContext context,
            ISongStorageService songStorageService,
            IAlbumStorageService albumStorageService)
        {
            _songStorageService = songStorageService;
            _context = context;
            _albumStorageService = albumStorageService;
        }

        public async ValueTask<Result<QueryResponse>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var song = await _context.Songs
                .AsNoTracking()
                .Include(x => x.Album)
                .Include(x => x.Artists)
                .ThenInclude(x => x.Artist)
                .Include(x => x.AllowedRegions)
                .Include(x => x.Genres)
                .SingleOrDefaultAsync(
                    x => x.Id == request.SongId,
                    cancellationToken);

            if (song is null)
            {
                return new Exception("Song not found");
            }
            
            if (song.Explicit && request.UserAge < UserConstants.AdultLegalAge)
            {
                return new Exception("User is not allowed to access explicit songs");
            }

            var s3SongPath = song.S3MediaFileName;
            var songUrlGetResult = await _songStorageService.GetPresignedUrl(s3SongPath);

            var s3AlbumArtPath = song.Album.S3ArtworkFilename;
            var albumArtUrlGetResult = await _albumStorageService.GetPresignedUrl(s3AlbumArtPath);

            return QueryResponse.FromEntity(
                song,
                songUrlGetResult.Match<string?>(url => url, _ => null),
                albumArtUrlGetResult.Match<string?>(url => url, _ => null),
                request.UserRegion);
        }
    }
}