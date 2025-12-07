using System.Text.Json.Serialization;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.QueryExtensions;
using MusicStreamingService.Extensions;
using MusicStreamingService.Infrastructure.Authentication;
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

    
    public sealed record Query : BasePaginatedRequest, IRequest<Result<QueryResponse, Exception>>
    {
        public Guid UserId { get; init; }
        
        public QueryBody Body { get; init; }
        
        public sealed record QueryBody : BasePaginatedRequest
        {
            
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

        public Handler(MusicStreamingContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<QueryResponse, Exception>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = _context.Songs
                .AsNoTracking()
                .Where(s => s.LikedByUsers.Any(u => u.Id == request.UserId));

            var totalCount = await query.CountAsync(cancellationToken);
            var songs = await query
                .Include(s => s.AllowedRegions)
                .Include(s => s.Genres)
                .Include(s => s.Artists)
                .ThenInclude(s => s.Artist)
                .OrderBy(s => s.Likes)
                .ApplyPagination(request.ItemsPerPage, request.Page)
                .ToListAsync(cancellationToken);

            return new QueryResponse
            {
                TotalCount = totalCount,
                ItemsPerPage = request.ItemsPerPage,
                ItemCount = songs.Count,
                Page = request.Page,
                Songs = songs.Select(ShortSongDto.FromEntity).ToList()
            };
        }
    }
}