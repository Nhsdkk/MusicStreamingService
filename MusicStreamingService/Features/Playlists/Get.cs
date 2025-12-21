using System.Text.Json.Serialization;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Common.Result;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Extensions;
using MusicStreamingService.Features.Songs;
using MusicStreamingService.Features.Users;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Playlists;

[ApiController]
public class Get : ControllerBase
{
    private readonly IMediator _mediator;

    public Get(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    /// <summary>
    /// Get full playlist data by ID
    /// </summary>
    /// <param name="request">Id of the playlist</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/playlists")]
    [Authorize(Roles = Permissions.ViewPlaylistsPermission)]
    [Tags(RouteGroups.Playlists)]
    [ProducesResponseType<QueryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPlaylist(
        [FromQuery] Query.QueryBody request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new Query
        {
            Body = request,
            UserId = User.GetUserId(),
            UserRegion = User.GetUserRegion(),
        }, cancellationToken);
        
        return result.Match<IActionResult>(Ok, BadRequest);
    }
    
    public sealed record Query : IRequest<Result<QueryResponse>>
    {
        public sealed record QueryBody
        {
            [FromQuery(Name = "playlistId")]
            public Guid PlaylistId { get; init; }
        }
        
        public QueryBody Body { get; init; } = null!;
        
        public Guid UserId { get; init; }

        public RegionClaim UserRegion { get; init; } = null!;
    }

    public sealed record QueryResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("title")]
        public string Title { get; init; } = null!;

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("creator")]
        public ShortUserDto Creator { get; init; } = null!;

        [JsonPropertyName("accessType")]
        public PlaylistAccessType AccessType { get; init; }

        [JsonPropertyName("likes")]
        public long Likes { get; init; }
        
        [JsonPropertyName("songs")]
        public List<ShortSongDto> Songs { get; init; } = null!;

        public static QueryResponse FromEntity(
            PlaylistEntity playlist,
            Dictionary<string, string?> albumArtworkMapping,
            RegionClaim regionClaim) =>
            new QueryResponse
            {
                Id = playlist.Id,
                Title = playlist.Title,
                Description = playlist.Description,
                Creator = ShortUserDto.FromEntity(playlist.Creator),
                AccessType = playlist.AccessType,
                Likes = playlist.Likes,
                Songs = playlist.Songs
                    .Select(ps => ShortSongDto.FromEntity(
                        ps.Song,
                        albumArtworkMapping[ps.Song.Album.S3ArtworkFilename],
                        regionClaim))
                    .ToList()
            };
    }
    
    public sealed class Handler : IRequestHandler<Query, Result<QueryResponse>>
    {
        private readonly MusicStreamingContext _context;
        private readonly IAlbumStorageService _albumStorageService;

        public Handler(
            MusicStreamingContext context,
            IAlbumStorageService albumStorageService)
        {
            _context = context;
            _albumStorageService = albumStorageService;
        }

        public async ValueTask<Result<QueryResponse>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var playlist = await _context.Playlists
                .AsNoTracking()
                .Include(x => x.Creator)
                .Include(x => x.Songs)
                .ThenInclude(x => x.Song)
                .ThenInclude(x => x.Artists)
                .ThenInclude(x => x.Artist)
                .Include(x => x.Songs)
                .ThenInclude(x => x.Song)
                .ThenInclude(x => x.AllowedRegions)
                .Include(x => x.Songs)
                .ThenInclude(x => x.Song)
                .ThenInclude(x => x.Genres)
                .Include(x => x.Songs)
                .ThenInclude(x => x.Song)
                .ThenInclude(x => x.Album)
                .SingleOrDefaultAsync(x => x.Id == request.Body.PlaylistId, cancellationToken);

            if (playlist is null)
            {
                return new Exception("Playlist not found");
            }

            if (playlist.AccessType is PlaylistAccessType.Private && playlist.CreatorId != request.UserId)
            {
                return new Exception("You do not have access to this playlist");
            }
            
            var albumArtworkFilenames = playlist.Songs
                .Select(x => x.Song.Album.S3ArtworkFilename)
                .ToList();

            var albumArtworkMapping = await _albumStorageService.GetPresignedUrls(
                albumArtworkFilenames,
                cancellationToken);

            return QueryResponse.FromEntity(playlist, albumArtworkMapping, request.UserRegion);
        }
    }
}