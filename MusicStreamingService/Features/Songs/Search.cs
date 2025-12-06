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
using MusicStreamingService.Infrastructure.Authentication;

namespace MusicStreamingService.Features.Songs;

[ApiController]
public class SearchSong : ControllerBase
{
    private readonly IMediator _mediator;

    public SearchSong(IMediator mediator)
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
            },
            cancellationToken);

        return Ok(results);
    }

    public sealed record Query : IRequest<QueryResponse>
    {
        public sealed record QueryBody
        {
            [JsonPropertyName("title")]
            public string? Title { get; init; }

            [JsonPropertyName("artistName")]
            public string? ArtistName { get; init; }

            [JsonPropertyName("allowExplicit")]
            public bool? AllowExplicit { get; init; }

            [JsonPropertyName("genres")]
            public List<Guid>? Genres { get; init; }

            [JsonPropertyName("itemsPerPage")]
            public int ItemsPerPage { get; init; } = 10;

            [JsonPropertyName("pageNumber")]
            public int PageNumber { get; init; } = 0;
            
            public sealed class Validator : AbstractValidator<QueryBody>
            {
                public Validator()
                {
                    RuleFor(x => x.ItemsPerPage).GreaterThan(0);
                    RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(0);
                    RuleFor(x => x.Genres).NotEmpty().When(x => x.Genres is not null);
                    RuleFor(x => x.Title).NotEmpty().When(x => x.Title is not null);
                    RuleFor(x => x.ArtistName).NotEmpty().When(x => x.ArtistName is not null);
                }
            }
        }

        public QueryBody Body { get; init; } = null!;

        public RegionClaim Region { get; init; } = null!;
    }

    public sealed record QueryResponse
    {
        public sealed record ShortSongDto
        {
            public sealed record ShortArtistInfoDto
            {
                [JsonPropertyName("id")]
                public Guid Id { get; init; }

                [JsonPropertyName("username")]
                public string Username { get; init; } = null!;
                
                [JsonPropertyName("mainArtist")]
                public bool MainArtist { get; init; }

                public static ShortArtistInfoDto FromEntity(UserEntity artist, bool mainArtist) =>
                    new ShortArtistInfoDto
                    {
                        Id = artist.Id,
                        Username = artist.Username,
                        MainArtist = mainArtist
                    };
            }

            public sealed record GenreDto
            {
                [JsonPropertyName("id")]
                public Guid Id { get; init; }

                [JsonPropertyName("title")]
                public string Title { get; init; } = null!;

                public static GenreDto FromEntity(GenreEntity genre) =>
                    new GenreDto
                    {
                        Id = genre.Id,
                        Title = genre.Title
                    };
            }

            public sealed record RegionDto
            {
                [JsonPropertyName("id")]
                public Guid Id { get; init; }

                [JsonPropertyName("title")]
                public string Title { get; init; } = null!;

                public static RegionDto FromEntity(RegionEntity region) =>
                    new RegionDto
                    {
                        Id = region.Id,
                        Title = region.Title
                    };
            }

            [JsonPropertyName("id")]
            public Guid Id { get; init; }

            [JsonPropertyName("title")]
            public string Title { get; init; } = null!;

            [JsonPropertyName("artists")]
            public List<ShortArtistInfoDto> Artists { get; init; } = null!;

            [JsonPropertyName("durationMs")]
            public long DurationMs { get; init; }

            [JsonPropertyName("likes")]
            public long Likes { get; init; }

            [JsonPropertyName("isExplicit")]
            public bool IsExplicit { get; init; }

            [JsonPropertyName("genres")]
            public List<GenreDto> Genres { get; init; } = null!;

            [JsonPropertyName("allowedRegions")]
            public List<RegionDto> AllowedRegions { get; init; } = null!;

            public static ShortSongDto FromEntity(SongEntity song) =>
                new ShortSongDto
                {
                    Id = song.Id,
                    Title = song.Title,
                    Artists = song.Artists
                        .Select(x => ShortArtistInfoDto.FromEntity(x.Artist, x.MainArtist))
                        .ToList(),
                    DurationMs = song.DurationMs,
                    Likes = song.Likes,
                    IsExplicit = song.Explicit,
                    Genres = song.Genres
                        .Select(GenreDto.FromEntity)
                        .ToList(),
                    AllowedRegions = song.AllowedRegions
                        .Select(RegionDto.FromEntity)
                        .ToList()
                };
        }

        [JsonPropertyName("songs")]
        public List<ShortSongDto> Songs { get; init; } = null!;

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; init; }

        [JsonPropertyName("itemsPerPage")]
        public int ItemsPerPage { get; init; }

        [JsonPropertyName("itemCount")]
        public int ItemCount { get; init; }

        [JsonPropertyName("page")]
        public int Page { get; init; }
    }

    public sealed class Handler : IRequestHandler<Query, QueryResponse>
    {
        private readonly MusicStreamingContext _context;

        public Handler(
            MusicStreamingContext context)
        {
            _context = context;
        }

        public async ValueTask<QueryResponse> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
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
            var songs = await query
                .Include(x => x.AllowedRegions)
                .Include(x => x.Artists)
                .ThenInclude(x => x.Artist)
                .Include(x => x.Genres)
                .Skip(requestBody.PageNumber * requestBody.ItemsPerPage)
                .Take(requestBody.ItemsPerPage)
                .ToListAsync(cancellationToken);

            return new QueryResponse
            {
                Songs = songs.Select(QueryResponse.ShortSongDto.FromEntity).ToList(),
                TotalCount = totalCount,
                ItemsPerPage = requestBody.ItemsPerPage,
                ItemCount = songs.Count,
                Page = requestBody.PageNumber
            };
        }
    }
}