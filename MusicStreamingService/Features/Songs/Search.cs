using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Data.QueryExtensions;
using MusicStreamingService.Extensions;
using MusicStreamingService.Features.Users;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;
using MusicStreamingService.Requests;
using MusicStreamingService.Responses;

namespace MusicStreamingService.Features.Songs;

[ApiController]
public class Search : ControllerBase
{
    private readonly IMediator _mediator;

    public Search(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search songs by various filters
    /// </summary>
    /// <param name="query">Filters</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/songs/search")]
    [Tags(RouteGroups.Songs)]
    [ProducesResponseType<QueryResponse>(StatusCodes.Status200OK)]
    [Authorize(Roles = Permissions.ViewSongsPermission)]
    public async Task<IActionResult> SearchSongs(
        [FromQuery] Query.QueryBody query,
        CancellationToken cancellationToken = default)
    {
        var results = await _mediator.Send(
            new Query
            {
                Body = query,
                Region = User.GetUserRegion(),
                Age = User.GetUserAge()
            },
            cancellationToken);

        return results.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record Query : IRequest<Result<QueryResponse>>
    {
        public sealed record QueryBody : BasePaginatedRequest
        {
            [JsonPropertyName("title")]
            public string? Title { get; init; }

            [JsonPropertyName("artistName")]
            public string? ArtistName { get; init; }

            [JsonPropertyName("allowExplicit")]
            public bool? AllowExplicit { get; init; }

            [JsonPropertyName("genres")]
            public List<Guid>? Genres { get; init; }

            public sealed class Validator : AbstractValidator<QueryBody>
            {
                public Validator()
                {
                    RuleFor(x => x.ItemsPerPage).GreaterThan(0).LessThan(100);
                    RuleFor(x => x.Page).GreaterThanOrEqualTo(0);
                    RuleFor(x => x.Genres).NotEmpty().When(x => x.Genres is not null);
                    RuleFor(x => x.Title).NotEmpty().When(x => x.Title is not null);
                    RuleFor(x => x.ArtistName).NotEmpty().When(x => x.ArtistName is not null);
                }
            }
        }

        public QueryBody Body { get; init; } = null!;

        public RegionClaim Region { get; init; } = null!;

        public int Age { get; init; }
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
            MusicStreamingContext context, IAlbumStorageService albumStorageService)
        {
            _context = context;
            _albumStorageService = albumStorageService;
        }

        public async ValueTask<Result<QueryResponse>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            if (request.Body.AllowExplicit == true && request.Age < UserConstants.AdultLegalAge)
            {
                return new Exception($"Users under {UserConstants.AdultLegalAge} years old are not allowed to search for explicit songs");
            }

            var requestBody = request.Body;
            var userRegion = request.Region;

            var query = _context.Songs
                .AsNoTracking()
                .Where(s => s.AllowedRegions.Any(region => region.Id == userRegion.Id))
                .FilterByOptionalArtistName(requestBody.ArtistName)
                .FilterByOptionalTitle(requestBody.Title)
                .FilterByOptionalGenres(requestBody.Genres)
                .EnableExplicit(requestBody.AllowExplicit);

            var totalCount = await query.CountAsync(cancellationToken);
            // TODO: Optimize query to use split query (Microsoft.EntityFrameworkCore.Query[20504])
            // TODO: Look into Microsoft.EntityFrameworkCore.Query[10102]
            var songs = await query
                .Include(x => x.AllowedRegions)
                .Include(x => x.Album)
                .Include(x => x.Artists)
                .ThenInclude(x => x.Artist)
                .Include(x => x.Genres)
                .OrderByDescending(x => x.Likes)
                .Skip(requestBody.Page * requestBody.ItemsPerPage)
                .Take(requestBody.ItemsPerPage)
                .ToListAsync(cancellationToken);

            var albumArtPaths = songs.Select(x => x.Album.S3ArtworkFilename);
            var albumArtUrls = await _albumStorageService.GetPresignedUrls(albumArtPaths);

            return new QueryResponse
            {
                Songs = songs
                    .Select(s =>
                        ShortSongDto.FromEntity(s, albumArtUrls[s.Album.S3ArtworkFilename])
                    ).ToList(),
                TotalCount = totalCount,
                ItemsPerPage = requestBody.ItemsPerPage,
                ItemCount = songs.Count,
                Page = requestBody.Page
            };
        }
    }
}