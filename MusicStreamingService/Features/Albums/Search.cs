using System.Text.Json.Serialization;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Data.QueryExtensions;
using MusicStreamingService.Features.Users;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.DateUtils;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;
using MusicStreamingService.Requests;
using MusicStreamingService.Responses;

namespace MusicStreamingService.Features.Albums;

[ApiController]
public sealed class Search : ControllerBase
{
    private readonly IMediator _mediator;

    public Search(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search albums by various filters
    /// </summary>
    /// <param name="request">Search filters</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/albums/search")]
    [Tags(RouteGroups.Albums)]
    [Authorize(Roles = Permissions.ViewAlbumsPermission)]
    [ProducesResponseType(typeof(QueryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchAlbums(
        [FromQuery] Query request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    public sealed record Query : BasePaginatedRequest, IRequest<Result<QueryResponse>>
    {
        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("artistName")]
        public string? ArtistName { get; init; }

        [JsonPropertyName("releaseDateRange")]
        public DateRange? ReleaseDateRange { get; init; }
    }

    public sealed record QueryResponse : BasePaginatedResponse
    {
        public sealed record ShortAlbumInfo
        {
            public Guid Id { get; init; }

            public string Title { get; init; } = null!;

            public long Likes { get; init; }

            public ShortAlbumCreatorDto AlbumCreator { get; init; } = null!;

            public string? ArtworkUrl { get; init; } = null!;

            public static ShortAlbumInfo FromEntity(
                AlbumEntity album,
                string? artworkUrl) =>
                new ShortAlbumInfo
                {
                    Id = album.Id,
                    Title = album.Title,
                    Likes = album.Likes,
                    ArtworkUrl = artworkUrl,
                    AlbumCreator = ShortAlbumCreatorDto.FromEntity(album.Artist)
                };
        }

        public List<ShortAlbumInfo> Albums { get; init; } = new();

        public static QueryResponse FromEntity(
            int totalCount,
            Query request,
            List<AlbumEntity> albums,
            Dictionary<string, string?> albumArtworkUrlMapping) =>
            new QueryResponse
            {
                TotalCount = totalCount,
                ItemsPerPage = request.ItemsPerPage,
                ItemCount = albums.Count,
                Page = request.Page,
                Albums = albums
                    .Select(x => ShortAlbumInfo.FromEntity(
                        x,
                        albumArtworkUrlMapping[x.S3ArtworkFilename]))
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

        public async ValueTask<Result<QueryResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var albumsSearchQuery = _context.Albums
                .AsNoTracking()
                .FilterByOptionalAlbumCreator(request.ArtistName)
                .FilterByOptionalReleaseDate(request.ReleaseDateRange)
                .FilterByOptionalTitle(request.Title);

            var totalCount = await albumsSearchQuery.CountAsync(cancellationToken);
            var albums = await albumsSearchQuery
                .ApplyPagination(request.ItemsPerPage, request.Page)
                .ToListAsync(cancellationToken);

            var albumArtworkUrlsMapping =
                await _albumStorageService.GetPresignedUrls(albums.Select(x => x.S3ArtworkFilename));

            return QueryResponse.FromEntity(totalCount, request, albums, albumArtworkUrlsMapping);
        }
    }
}