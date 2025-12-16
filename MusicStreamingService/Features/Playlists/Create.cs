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
using MusicStreamingService.Features.Songs;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Playlists;

[ApiController]
public class Create : ControllerBase
{
    private readonly IMediator _mediator;

    public Create(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create playlist with provided songs
    /// </summary>
    /// <param name="request">Playlist data</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/playlists")]
    [Tags(RouteGroups.Playlists)]
    [Authorize(Roles = Permissions.ManagePlaylistsPermission)]
    [ProducesResponseType<CommandResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePlaylist(
        Command.CommandBody request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Command
            {
                Body = request,
                UserId = User.GetUserId()
            },
            cancellationToken);

        return result.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record Command : ITransactionWrappedCommand<Result<CommandResponse>>
    {
        public sealed record CommandBody
        {
            [JsonPropertyName("title")]
            public string Title { get; init; } = null!;

            [JsonPropertyName("description")]
            public string? Description { get; init; }

            [JsonPropertyName("accessType")]
            public PlaylistAccessType AccessType { get; init; }

            [JsonPropertyName("songs")]
            public List<Guid> Songs { get; init; } = null!;

            public sealed class Validator : AbstractValidator<CommandBody>
            {
                public Validator()
                {
                    RuleFor(x => x.Title)
                        .NotEmpty()
                        .MaximumLength(200);
                    RuleFor(x => x.Description)
                        .NotEmpty()
                        .When(x => x.Description is not null);
                    RuleFor(x => x.Songs)
                        .NotNull()
                        .NotEmpty();

                    RuleFor(x => x.Songs)
                        .Must(x => x.Distinct().Count() == x.Count)
                        .WithMessage("Songs in the playlist must be unique.");
                }
            }
        }

        public CommandBody Body { get; init; } = null!;

        public Guid UserId { get; init; }
    }

    public sealed record CommandResponse
    {
        public sealed record PlaylistCreator
        {
            [JsonPropertyName("id")]
            public Guid Id { get; init; }

            [JsonPropertyName("username")]
            public string Username { get; init; } = null!;
        }

        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("title")]
        public string Title { get; init; } = null!;

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("creator")]
        public PlaylistCreator Creator { get; init; } = null!;

        [JsonPropertyName("accessType")]
        public PlaylistAccessType AccessType { get; init; }

        [JsonPropertyName("likes")]
        public long Likes { get; init; }

        [JsonPropertyName("songs")]
        public List<ShortSongDto> Songs { get; init; } = null!;

        public static CommandResponse FromEntity(
            PlaylistEntity playlist,
            Dictionary<string, string?> albumArtworkUrlMapping)
        {
            return new CommandResponse
            {
                Id = playlist.Id,
                Title = playlist.Title,
                Description = playlist.Description,
                Creator = new PlaylistCreator
                {
                    Id = playlist.Creator.Id,
                    Username = playlist.Creator.Username
                },
                AccessType = playlist.AccessType,
                Likes = playlist.Likes,
                Songs = playlist.Songs
                    .OrderBy(x => x.AddedAt)
                    .Select(x => ShortSongDto.FromEntity(x.Song, albumArtworkUrlMapping[x.Song.Album.S3ArtworkFilename]))
                    .ToList()
            };
        }
    }

    public sealed class Handler : IRequestHandler<Command, Result<CommandResponse>>
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

        public async ValueTask<Result<CommandResponse>> Handle(
            Command request,
            CancellationToken cancellationToken = default)
        {
            var requestBody = request.Body;

            var songIds = requestBody.Songs.ToHashSet();
            var songs = await _context.Songs
                .Include(x => x.Album)
                .Include(x => x.AllowedRegions)
                .Include(x => x.Artists)
                .ThenInclude(x => x.Artist)
                .Include(x => x.Genres)
                .Where(x => songIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            if (songs.Count != songIds.Count)
            {
                return new Exception("One or more songs do not exist.");
            }

            var playlist = new PlaylistEntity
            {
                Title = requestBody.Title,
                Description = requestBody.Description,
                CreatorId = request.UserId,
                AccessType = requestBody.AccessType,
                Songs = []
            };

            playlist.Songs = songs.Select(song => new PlaylistSongEntity
            {
                Playlist = playlist,
                Song = song,
            }).ToList();

            await _context.Playlists.AddAsync(playlist, cancellationToken);
            await _context.PlaylistSongs.AddRangeAsync(playlist.Songs, cancellationToken);

            var albumArtworkUrlMapping =
                await _albumStorageService.GetPresignedUrls(songs.Select(x => x.Album.S3ArtworkFilename));
            
            await _context.SaveChangesAsync(cancellationToken);

            await _context.Entry(playlist).Reference(x => x.Creator).LoadAsync(cancellationToken);
            
            return CommandResponse.FromEntity(playlist, albumArtworkUrlMapping);
        }
    }
}