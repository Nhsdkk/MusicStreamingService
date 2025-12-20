using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Common.Result;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Data.QueryExtensions;
using MusicStreamingService.Extensions;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Openapi;
using MusicStreamingService.Requests;
using MusicStreamingService.Responses;
using MusicStreamingService.Validators;

namespace MusicStreamingService.Features.Albums;

[ApiController]
public class GetOwned : ControllerBase
{
    private readonly IMediator _mediator;

    public GetOwned(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/api/v1/albums/owned")]
    [Authorize(Roles = Permissions.ViewAlbumsPermission)]
    [Tags(RouteGroups.Albums)]
    [ProducesResponseType<QueryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOwnedAlbums(
        [FromQuery] Query request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record Query : BasePaginatedRequest, IRequest<Result<QueryResponse>>
    {
        [JsonPropertyName("userId")]
        public Guid UserId { get; init; }
            
        public sealed class Validator : BasePaginatedRequestValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.UserId).NotEmpty();
            }
        }
    }

    public sealed record QueryResponse : BasePaginatedResponse
    {
        [JsonPropertyName("albums")]
        public List<ShortAlbumDto> Albums { get; init; } = new();

        public static QueryResponse FromEntity(
            List<AlbumEntity> albums,
            int totalCount,
            Query request,
            Dictionary<string, string?> albumArtworkMapping) =>
            new QueryResponse
            {
                Albums = albums
                    .Select(x => ShortAlbumDto.FromEntity(x, albumArtworkMapping[x.S3ArtworkFilename]))
                    .ToList(),
                TotalCount = totalCount,
                ItemsPerPage = request.ItemsPerPage,
                Page = request.ItemsPerPage,
                ItemCount = albums.Count,
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
            var userIsArtist = await _context.Users
                .AsNoTracking()
                .Include(x => x.Roles)
                .ThenInclude(x => x.Permissions)
                .Select(x => x.Roles.Any(r => r.Permissions.Any(p => p.Title == Permissions.ManageAlbumsPermission)))
                .SingleOrDefaultAsync(cancellationToken);

            if (!userIsArtist)
            {
                return new Exception("Can't find artist with the given user ID");
            }
                
            var query = _context.Albums
                .AsNoTracking()
                .Where(x => x.ArtistId == request.UserId);

            var totalCount = await query.CountAsync(cancellationToken);

            var albums = await query
                .OrderBy(x => x.CreatedAt)
                .ApplyPagination(request.ItemsPerPage, request.Page)
                .ToListAsync(cancellationToken);

            var albumArtworkFilenames = albums.Select(x => x.S3ArtworkFilename).ToList();
            var albumArtworkMappings = await _albumStorageService.GetPresignedUrls(
                albumArtworkFilenames,
                cancellationToken);

            return QueryResponse.FromEntity(albums, totalCount, request, albumArtworkMappings);
        }
    }
}