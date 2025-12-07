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

namespace MusicStreamingService.Features.Songs;

[ApiController]
public sealed class GetFavorite : ControllerBase
{
    private readonly IMediator _mediator;

    public GetFavorite(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get user's favorite songs
    /// </summary>
    /// <param name="request">Pagination params</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/songs/favorite")]
    [Tags(RouteGroups.Songs)]
    [Authorize(Roles = Permissions.ViewSongsPermission)]
    [ProducesResponseType<QueryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFavoriteSongs(
        [FromQuery] Query.QueryBody request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Query
            {
                UserId = User.GetUserId(),
                Body = request
            },
            cancellationToken);

        return result.Match<IActionResult>(Ok, BadRequest);
    }


    public sealed record Query : IRequest<Result<QueryResponse, Exception>>
    {
        public Guid UserId { get; init; }

        public QueryBody Body { get; init; } = null!;

        public sealed record QueryBody : BasePaginatedRequest
        {
        }

        public sealed class Validator : AbstractValidator<QueryBody>
        {
            public Validator()
            {
                RuleFor(x => x.ItemsPerPage).GreaterThan(0).LessThan(100);
                RuleFor(x => x.Page).GreaterThanOrEqualTo(0);
            }
        }
    }

    public sealed record QueryResponse : BasePaginatedResponse
    {
        [JsonPropertyName("songs")]
        public List<ShortSongDto> Songs { get; init; } = null!;
    }

    public sealed class Handler : IRequestHandler<Query, Result<QueryResponse, Exception>>
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

        public async ValueTask<Result<QueryResponse, Exception>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = _context.Songs
                .AsNoTracking()
                .Where(s => s.LikedByUsers.Any(u => u.Id == request.UserId));

            var totalCount = await query.CountAsync(cancellationToken);
            var songs = await query
                .Include(s => s.Album)
                .Include(s => s.AllowedRegions)
                .Include(s => s.Genres)
                .Include(s => s.Artists)
                .ThenInclude(s => s.Artist)
                .OrderByDescending(s => s.Likes)
                .ApplyPagination(request.Body.ItemsPerPage, request.Body.Page)
                .ToListAsync(cancellationToken);

            var albumArtPaths = songs.Select(x => x.Album.S3ArtworkFilename);
            var albumArtUrls = await _albumStorageService.GetPresignedUrls(albumArtPaths);

            return new QueryResponse
            {
                TotalCount = totalCount,
                ItemsPerPage = request.Body.ItemsPerPage,
                ItemCount = songs.Count,
                Page = request.Body.Page,
                Songs = songs
                    .Select(s =>
                        ShortSongDto.FromEntity(s, albumArtUrls[s.Album.S3ArtworkFilename])
                    ).ToList()
            };
        }
    }
}