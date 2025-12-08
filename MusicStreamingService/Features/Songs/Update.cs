using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Commands;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Extensions;
using MusicStreamingService.Features.Albums;
using MusicStreamingService.Features.Genres;
using MusicStreamingService.Features.Region;
using MusicStreamingService.Features.Users;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Songs;

[ApiController]
public sealed class Update : ControllerBase
{
    private readonly IMediator _mediator;

    public Update(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("/api/v1/songs")]
    [Tags(RouteGroups.Songs)]
    [Authorize(Roles = Permissions.ManageSongsPermission)]
    [ProducesResponseType<CommandResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle(
        [FromBody] Command.CommandBody request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new Command
            {
                Body = request,
                UserId = User.GetUserId()
            },
            cancellationToken);

        return result.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record Command : ITransactionWrappedCommand<Result<CommandResponse, Exception>>
    {
        public sealed record CommandBody
        {
            [JsonPropertyName("id")]
            public Guid Id { get; init; }

            [JsonPropertyName("title")]
            public string? Title { get; init; }

            [JsonPropertyName("explicit")]
            public bool? Explicit { get; init; }

            [JsonPropertyName("artistIds")]
            public List<Guid>? ArtistIds { get; init; }

            [JsonPropertyName("genreIds")]
            public List<Guid>? GenreIds { get; init; }
        }

        public CommandBody Body { get; init; } = null!;

        public Guid UserId { get; init; }

        public sealed class Validator : AbstractValidator<CommandBody>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Title).NotEmpty().When(x => x.Title is not null);
                RuleFor(x => x.ArtistIds).NotEmpty().When(x => x.ArtistIds is not null);
                RuleFor(x => x.ArtistIds)
                    .Must(x => x!.Distinct().Count() == x!.Count)
                    .When(x => x.ArtistIds is not null)
                    .WithMessage("Artist IDs must be unique");
                RuleFor(x => x.GenreIds).NotEmpty().When(x => x.GenreIds is not null);
                RuleFor(x => x.GenreIds)
                    .Must(x => x!.Distinct().Count() == x!.Count)
                    .When(x => x.GenreIds is not null)
                    .WithMessage("Genre IDs must be unique");
            }
        }
    }

    public sealed record CommandResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("title")]
        public string Title { get; init; } = null!;

        [JsonPropertyName("durationMs")]
        public long DurationMs { get; init; }

        [JsonPropertyName("likes")]
        public long Likes { get; init; }

        [JsonPropertyName("explicit")]
        public bool Explicit { get; init; }

        // TODO: Remove and move to album route group
        [JsonPropertyName("isTitleTrack")]
        public bool IsTitleTrack { get; init; }

        // TODO: Remove and move to album route group
        [JsonPropertyName("albumPosition")]
        public long AlbumPosition { get; init; }

        [JsonPropertyName("artists")]
        public List<ShortSongArtistDto> Artists { get; init; } = null!;

        [JsonPropertyName("genres")]
        public List<ShortGenreDto> Genres { get; init; } = null!;

        [JsonPropertyName("album")]
        public ShortAlbumDto Album { get; init; } = null!;

        [JsonPropertyName("allowedRegions")]
        public List<ShortRegionDto> AllowedRegions { get; init; } = null!;

        public static CommandResponse FromEntity(
            SongEntity song,
            string albumCoverUrl) =>
            new CommandResponse
            {
                Id = song.Id,
                Title = song.Title,
                DurationMs = song.DurationMs,
                Likes = song.Likes,
                Explicit = song.Explicit,
                IsTitleTrack = song.IsTitleTrack,
                AlbumPosition = song.AlbumPosition,
                Artists = song.Artists
                    .Select(x =>
                        ShortSongArtistDto.FromEntity(x.Artist, x.MainArtist)
                    ).ToList(),
                Genres = song.Genres.Select(ShortGenreDto.FromEntity).ToList(),
                Album = ShortAlbumDto.FromEntity(
                    song.Album,
                    albumCoverUrl),
                AllowedRegions = song.AllowedRegions.Select(ShortRegionDto.FromEntity).ToList()
            };
    }

    public sealed class Handler : IRequestHandler<Command, Result<CommandResponse, Exception>>
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

        public async ValueTask<Result<CommandResponse, Exception>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var body = request.Body;
            var userId = request.UserId;

            if (body.ArtistIds is not null && body.ArtistIds.All(id => id != userId))
            {
                return new Exception("Can't remove yourself from artist list");
            }

            var song = await _context.Songs
                .Include(s => s.Artists)
                .ThenInclude(s => s.Artist)
                .Include(s => s.Album)
                .Include(s => s.Genres)
                .Include(s => s.AllowedRegions)
                .SingleOrDefaultAsync(
                    s => s.Id == body.Id && s.Artists.Any(a => a.Artist.Id == userId && a.MainArtist),
                    cancellationToken);

            if (song is null)
            {
                return new Exception("Song not found");
            }

            song.Title = body.Title ?? song.Title;
            song.Explicit = body.Explicit ?? song.Explicit;

            if (body.ArtistIds is not null)
            {
                var artists = await _context.Users
                    .Where(a =>
                        body.ArtistIds.Contains(a.Id) &&
                        !a.Disabled &&
                        a.Roles.Any(r =>
                            r.Permissions.Any(p => p.Title == Permissions.ManageSongsPermission)
                        )
                    )
                    .ToListAsync(cancellationToken);

                if (artists.Count != body.ArtistIds.Count)
                {
                    var diff = body.ArtistIds.Where(aId => artists.All(a => a.Id != aId));
                    var stringDiff = string.Join(", ", diff);
                    return new Exception($"One or more artists not found or invalid: [{stringDiff}]");
                }

                var currentArtistIds = song.Artists.Select(x => x.Artist.Id);
                var add = body.ArtistIds.Where(aId => currentArtistIds.All(caId => aId != caId));

                song.Artists = song.Artists.Where(a => body.ArtistIds.Contains(a.Artist.Id)).ToList();
                song.Artists =
                [
                    ..song.Artists, ..add.Select(aId => new SongArtistEntity
                    {
                        SongId = song.Id,
                        ArtistId = aId,
                        MainArtist = false
                    })
                ];
            }

            if (body.GenreIds is not null)
            {
                var genres = await _context.Genres
                    .Where(g => body.GenreIds.Contains(g.Id))
                    .ToListAsync(cancellationToken);

                if (genres.Count != body.GenreIds.Count)
                {
                    var diff = body.GenreIds.Where(gId => genres.All(g => g.Id != gId));
                    var stringDiff = string.Join(", ", diff);
                    return new Exception($"One or more genres not found: [{stringDiff}]");
                }

                song.Genres = genres;
            }

            var songUrlResult = await _albumStorageService.GetPresignedUrl(song.Album.S3ArtworkFilename);
            if (songUrlResult.IsT1)
            {
                return songUrlResult.AsT1;
            }

            await _context.SaveChangesAsync(cancellationToken);

            
            return CommandResponse.FromEntity(
                song,
                albumCoverUrl: songUrlResult.AsT0
            );
        }
    }
}