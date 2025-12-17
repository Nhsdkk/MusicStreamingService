using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.QueryExtensions;
using MusicStreamingService.Extensions;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;
using MusicStreamingService.Requests;
using MusicStreamingService.Responses;
using MusicStreamingService.Validators;

namespace MusicStreamingService.Features.Songs;

[ApiController]
public sealed class GetContributed : ControllerBase
{
    private readonly IMediator _mediator;

    public GetContributed(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get songs to which user has contributed
    /// </summary>
    /// <param name="request">Request filters</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/songs/contributed-songs")]
    [Tags(RouteGroups.Songs)]
    [Authorize(Roles = Permissions.ViewSongsPermission)]
    [ProducesResponseType(typeof(QueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Exception), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetContributedSongs(
        [FromQuery] Query.QueryBody request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Query
            {
                UserRegion = User.GetUserRegion(),
                Body = request,
            },
            cancellationToken);

        return result.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record Query : IRequest<Result<QueryResponse>>
    {
        public sealed record QueryBody : BasePaginatedRequest
        {
            [JsonPropertyName("userId")]
            public Guid UserId { get; init; }
        }
        
        public RegionClaim UserRegion { get; init; } = null!;

        public QueryBody Body { get; init; } = null!;
        
        public sealed class Validator : BasePaginatedRequestValidator<QueryBody>
        {
            public Validator()
            {
                RuleFor(x => x.UserId).NotEmpty();
            }
        }
    }

    public sealed record QueryResponse : BasePaginatedResponse
    {
        [JsonPropertyName("songs")]
        public List<ShortSongDto> Songs { get; init; } = null!;
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
            var requestBody = request.Body;
            var user = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x =>
                        x.Id == requestBody.UserId,
                    cancellationToken);

            if (user == null || user.Disabled)
            {
                return new Exception("Artist not found");
            }

            var query = _context.Songs
                .AsNoTracking()
                .Where(s => s.Artists.Any(a => a.Artist.Id == user.Id));

            var totalCount = await query.CountAsync(cancellationToken);
            // TODO: Optimize query to use split query (Microsoft.EntityFrameworkCore.Query[20504])
            // TODO: Look into Microsoft.EntityFrameworkCore.Query[10102]
            var songs = await query
                .Include(s => s.Artists)
                .ThenInclude(a => a.Artist)
                .Include(s => s.AllowedRegions)
                .Include(s => s.Genres)
                .Include(s => s.Album)
                .OrderByDescending(x => x.Likes)
                .ApplyPagination(requestBody.ItemsPerPage, requestBody.Page)
                .ToListAsync(cancellationToken);

            var albumArtPaths = songs.Select(x => x.Album.S3ArtworkFilename);
            var albumArtUrls = await _albumStorageService.GetPresignedUrls(albumArtPaths);

            return new QueryResponse
            {
                Songs = songs
                    .Select(s =>
                        ShortSongDto.FromEntity(s, albumArtUrls[s.Album.S3ArtworkFilename], request.UserRegion)
                    ).ToList(),
                TotalCount = totalCount,
                ItemsPerPage = requestBody.ItemsPerPage,
                ItemCount = songs.Count,
                Page = requestBody.Page,
            };
        }
    }
}