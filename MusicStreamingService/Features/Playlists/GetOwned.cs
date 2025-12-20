using System.Text.Json.Serialization;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Data.QueryExtensions;
using MusicStreamingService.Extensions;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Openapi;
using MusicStreamingService.Requests;
using MusicStreamingService.Responses;
using MusicStreamingService.Validators;

namespace MusicStreamingService.Features.Playlists;

[ApiController]
public class GetOwned : ControllerBase
{
    private readonly IMediator _mediator;

    public GetOwned(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    /// <summary>
    /// Get user's playlists
    /// </summary>
    /// <param name="request">Paginated request data</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/playlists/search")]
    [Authorize(Roles = Permissions.ViewPlaylistsPermission)]
    [Tags(RouteGroups.Playlists)]
    [ProducesResponseType<QueryResponse>(200)]
    public async Task<IActionResult> GetOwnedPlaylists(
        [FromQuery] Query.QueryBody request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new Query
            {
                Body = request,
                UserId = User.GetUserId()
            }, cancellationToken);
        return Ok(result);
    }

    public sealed record Query : IRequest<QueryResponse>
    {
        public sealed record QueryBody : BasePaginatedRequest
        {
            public sealed class Validator : BasePaginatedRequestValidator<QueryBody>
            {
            }
        }
        
        public QueryBody Body { get; init; } = null!;
        
        public Guid UserId { get; init; }
    }

    public sealed record QueryResponse : BasePaginatedResponse
    {
        [JsonPropertyName("playlists")]
        public List<ShortPlaylistDto> Playlists { get; init; } = null!;
        
        public static QueryResponse FromEntity(
            List<PlaylistEntity> playlists,
            Query.QueryBody query,
            int totalCount) =>
            new QueryResponse
            {
                Playlists = playlists
                    .Select(ShortPlaylistDto.FromEntity)
                    .ToList(),
                TotalCount = totalCount,
                ItemsPerPage = query.ItemsPerPage,
                Page = query.Page,
                ItemCount = playlists.Count
            };
    }
    
    public sealed class Handler : IRequestHandler<Query, QueryResponse>
    {
        private readonly MusicStreamingContext _context;

        public Handler(MusicStreamingContext context)
        {
            _context = context;
        }

        public async ValueTask<QueryResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var requestBody = request.Body;
            var query = _context.Playlists
                .AsNoTracking()
                .Where(x => x.CreatorId == request.UserId);
            
            var totalCount = await query.CountAsync(cancellationToken);
            
            var playlists = await query
                .OrderByDescending(playlist => playlist.CreatedAt)
                .ApplyPagination(requestBody.ItemsPerPage, requestBody.Page)
                .Include(playlist => playlist.Creator)
                .ToListAsync(cancellationToken);

            return QueryResponse.FromEntity(playlists, requestBody, totalCount);
        }
    }
}