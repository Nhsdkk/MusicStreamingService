using System.Text.Json.Serialization;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Data.QueryExtensions;
using MusicStreamingService.Openapi;
using MusicStreamingService.Requests;
using MusicStreamingService.Responses;

namespace MusicStreamingService.Features.Genres;

[ApiController]
public sealed class Search : ControllerBase
{
    private readonly IMediator _mediator;

    public Search(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    /// <summary>
    /// Get genres by filters
    /// </summary>
    /// <param name="query">Title filter</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/genres")]
    [Tags(RouteGroups.Genres)]
    [ProducesResponseType<QueryResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchGenres(
        [FromQuery] Query query,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            query,
            cancellationToken);
        
        return Ok(result);
    }
    
    public sealed record Query : BasePaginatedRequest, IRequest<QueryResponse>
    {
        [JsonPropertyName("title")]
        public string? Title { get; init; } = null!;
    }

    public sealed record QueryResponse : BasePaginatedResponse
    {
        [JsonPropertyName("genres")]
        public List<GenreDto> Genres { get; init; } = null!;
        
        public static QueryResponse FromEntity(
            Query request,
            List<GenreEntity> genres,
            int totalCount) =>
            new QueryResponse
            {
                Genres = genres.Select(GenreDto.FromEntity).ToList(),
                Page = request.Page,
                ItemsPerPage = request.ItemsPerPage,
                TotalCount = totalCount,
                ItemCount = genres.Count,
            };
    }
    
    public sealed class Handler : IRequestHandler<Query, QueryResponse>
    {
        private readonly MusicStreamingContext _context;

        public Handler(MusicStreamingContext context)
        {
            _context = context;
        }

        public async ValueTask<QueryResponse> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var query = _context.Genres
                .AsNoTracking()
                .FilterByOptionalTitle(request.Title);
            
            var totalCount = await query.CountAsync(cancellationToken);
            var genres = await query
                .ApplyPagination(request.ItemsPerPage, request.Page)
                .ToListAsync(cancellationToken);

            return QueryResponse.FromEntity(request, genres, totalCount);
        }
    }
}