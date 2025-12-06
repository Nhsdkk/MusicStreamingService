using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;
using MusicStreamingService.Responses;

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
    [Authorize(Roles = Permissions.ViewUsersPermission)]
    [ProducesResponseType(typeof(QueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Exception), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetContributedSongs(
        [FromQuery] Query request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            request,
            cancellationToken);

        return result.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record Query : IRequest<Result<QueryResponse, Exception>>
    {
        [JsonPropertyName("userId")]
        public Guid UserId { get; init; }

        [JsonPropertyName("itemsPerPage")]
        public int ItemsPerPage { get; init; } = 10;

        [JsonPropertyName("page")]
        public int Page { get; init; } = 0;
        
        public sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.UserId).NotEmpty();
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

        public Handler(MusicStreamingContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<QueryResponse, Exception>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(x => x.Roles)
                .ThenInclude(x => x.Permissions)
                .SingleOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

            if (user == null || user.Disabled)
            {
                return new Exception("User not found");
            }

            var userIsArtist =
                user.Roles.Any(r => r.Permissions.Any(p => p.Title == Permissions.ManageSongsPermission));
            if (!userIsArtist)
            {
                return new Exception("User is not an artist");
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
                .OrderByDescending(x => x.Likes)
                .Skip(request.ItemsPerPage * request.Page)
                .Take(request.ItemsPerPage)
                .ToListAsync(cancellationToken);

            return new QueryResponse
            {
                Songs = songs.Select(ShortSongDto.FromEntity).ToList(),
                TotalCount = totalCount,
                ItemsPerPage = request.ItemsPerPage,
                ItemCount = songs.Count,
                Page = request.Page
            };
        }
    }
}