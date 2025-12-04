using System.Text.Json.Serialization;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;

namespace MusicStreamingService.Features.Users;

[ApiController]
public sealed class Search : ControllerBase
{
    private readonly IMediator _mediator;

    public Search(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("/api/v1/users/search")]
    [Authorize(Roles = "mss.users.view")]
    public async Task<IActionResult> SearchUsers(
        [FromQuery] Query query,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
    
    public sealed record Query : IRequest<QueryResponseDto>
    {
        [JsonPropertyName("username")]
        public string? Username { get; init; }

        [JsonPropertyName("itemsPerPage")]
        public int ItemsPerPage { get; init; } = 10;

        [JsonPropertyName("page")]
        public int Page { get; init; } = 0;
    }

    public sealed record QueryResponseDto
    {
        public sealed record RegionDto
        {
            [JsonPropertyName("id")]
            public Guid Id { get; init; }
            
            [JsonPropertyName("title")]
            public string Title { get; init; } = null!;
        }
        
        public sealed record UserSearchDataDto
        {
            [JsonPropertyName("id")]
            public Guid Id { get; init; }
            
            [JsonPropertyName("username")]
            public string Username { get; init; } = null!;
            
            [JsonPropertyName("region")]
            public RegionDto Region { get; init; } = null!;
        }
        
        [JsonPropertyName("users")]
        public List<UserSearchDataDto> Users { get; init; } = new();
        
        [JsonPropertyName("totalCount")]
        public long TotalCount { get; init; }
        
        [JsonPropertyName("itemCount")]
        public long ItemCount { get; init; }
        
        [JsonPropertyName("page")]
        public long Page { get; init; }
        
        [JsonPropertyName("itemsPerPage")]
        public long ItemsPerPage { get; init; }
    }
    
    internal sealed class Handler : IRequestHandler<Query, QueryResponseDto>
    {
        private readonly MusicStreamingContext _context;
        
        public Handler(
            MusicStreamingContext context)
        {
            _context = context;
        }
        
        public async ValueTask<QueryResponseDto> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var query = _context.Users
                .AsNoTracking();

            if (request.Username is not null)
            {
                query = query.Where(x => x.Username.Contains(request.Username));
            }
            
            var totalCount = await query.CountAsync(cancellationToken);

            var limit = request.ItemsPerPage;
            var offset = request.Page * request.ItemsPerPage;
            var users = await query
                .Include(x => x.Region)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(cancellationToken);

            var usersMapped = users.Select(x => new QueryResponseDto.UserSearchDataDto
            {
                Id = x.Id,
                Username = x.Username,
                Region = new QueryResponseDto.RegionDto
                {
                    Id = x.Region.Id,
                    Title = x.Region.Title
                }
            }).ToList();

            return new QueryResponseDto
            {
                Users = usersMapped,
                TotalCount = totalCount,
                ItemCount = usersMapped.Count,
                Page = request.Page
            };
        }
    }
    
}