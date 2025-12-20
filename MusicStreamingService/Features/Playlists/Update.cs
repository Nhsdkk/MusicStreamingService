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
using MusicStreamingService.Common.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Playlists;

[ApiController]
public sealed class Update : ControllerBase
{
    private readonly IMediator _mediator;

    public Update(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Update playlist details and songs
    /// </summary>
    /// <param name="request">New playlist details and songs</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("/api/v1/playlists")]
    [Tags(RouteGroups.Playlists)]
    [Authorize(Roles = Permissions.ManagePlaylistsPermission)]
    [ProducesResponseType<CommandResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle(
        [FromBody] Command.CommandBody request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new Command
            {
                Body = request,
                UserId = User.GetUserId(),
                UserRegion = User.GetUserRegion(),
            },
            cancellationToken);

        return result.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record Command : ITransactionWrappedCommand<Result<CommandResponse>>
    {
        public sealed record CommandBody
        {
            [JsonPropertyName("id")]
            public Guid Id { get; init; }

            [JsonPropertyName("title")]
            public string? Title { get; init; }

            [JsonPropertyName("description")]
            public string? Description { get; init; }

            [JsonPropertyName("descriptionFilled")]
            public bool DescriptionFilled { get; init; }

            [JsonPropertyName("songsToAdd")]
            public List<Guid>? SongsToAdd { get; init; }

            [JsonPropertyName("songsToRemove")]
            public List<Guid>? SongsToRemove { get; init; }

            public sealed class Validator : AbstractValidator<CommandBody>
            {
                public Validator()
                {
                    RuleFor(x => x.Id)
                        .NotEmpty();
                    RuleFor(x => x.Title)
                        .NotEmpty()
                        .MaximumLength(255)
                        .When(x => x.Title is not null);
                    RuleFor(x => x.Description)
                        .NotEmpty()
                        .When(x => x.Description is not null);
                    RuleFor(x => x.SongsToAdd)
                        .NotEmpty()
                        .When(x => x.SongsToAdd is not null);
                    RuleFor(x => x.SongsToRemove)
                        .NotEmpty()
                        .When(x => x.SongsToRemove is not null);

                    RuleFor(x => x.SongsToAdd)
                        .Must(x => x!.Distinct().Count() == x!.Count)
                        .When(x => x.SongsToAdd is not null)
                        .WithMessage("Songs to add must be unique.");
                    RuleFor(x => x.SongsToRemove)
                        .Must(x => x!.Distinct().Count() == x!.Count)
                        .When(x => x.SongsToRemove is not null)
                        .WithMessage("Songs to remove must be unique.");
                }
            }
        }

        public CommandBody Body { get; init; } = null!;

        public Guid UserId { get; init; }

        public RegionClaim UserRegion { get; init; } = null!;
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
            Dictionary<string, string?> albumArtworkUrlMapping,
            RegionClaim userRegion)
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
                    .Select(x => ShortSongDto.FromEntity(x.Song, albumArtworkUrlMapping[x.Song.Album.S3ArtworkFilename],
                        userRegion))
                    .ToList()
            };
        }
    }

    public sealed class Handler : IRequestHandler<Command, Result<CommandResponse>>
    {
        private readonly MusicStreamingContext _context;
        private readonly IAlbumStorageService _albumStorageService;

        public Handler(MusicStreamingContext context, IAlbumStorageService albumStorageService)
        {
            _context = context;
            _albumStorageService = albumStorageService;
        }

        public async ValueTask<Result<CommandResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            var requestBody = request.Body;
            var playlist = await _context.Playlists
                .Include(x => x.Creator)
                .Include(x => x.Songs)
                .ThenInclude(x => x.Song)
                .ThenInclude(x => x.Album)
                .Include(x => x.Songs)
                .ThenInclude(x => x.Song)
                .ThenInclude(x => x.Genres)
                .Include(x => x.Songs)
                .ThenInclude(x => x.Song)
                .ThenInclude(x => x.Artists)
                .ThenInclude(x => x.Artist)
                .Include(x => x.Songs)
                .ThenInclude(x => x.Song)
                .ThenInclude(x => x.AllowedRegions)
                .SingleOrDefaultAsync(x => x.Id == requestBody.Id, cancellationToken);

            if (playlist is null)
            {
                return new Exception("Playlist not found");
            }

            if (playlist.Creator.Id != request.UserId)
            {
                return new Exception("You are not the creator of this playlist");
            }

            playlist.Title = requestBody.Title ?? playlist.Title;
            playlist.Description = requestBody.DescriptionFilled ? requestBody.Description : playlist.Description;

            var storedSongIds = playlist.Songs.Select(x => x.Song.Id).ToHashSet();
            if (requestBody.SongsToRemove is not null)
            {
                if (!requestBody.SongsToRemove.All(x => storedSongIds.Contains(x)))
                {
                    return new Exception("One or more songs to remove are not in the playlist");
                }

                playlist.Songs.RemoveAll(x => requestBody.SongsToRemove.Contains(x.Song.Id));
                storedSongIds.RemoveWhere(x => requestBody.SongsToRemove.Contains(x));
            }

            if (requestBody.SongsToAdd is not null)
            {
                var songsToAdd = await _context.Songs
                    .Where(x => requestBody.SongsToAdd.Contains(x.Id))
                    .Include(x => x.Genres)
                    .Include(x => x.Artists)
                    .ThenInclude(x => x.Artist)
                    .Include(x => x.AllowedRegions)
                    .ToListAsync(cancellationToken);

                if (songsToAdd.Count != requestBody.SongsToAdd.Count)
                {
                    return new Exception("One or more songs to add were not found");
                }

                foreach (var song in songsToAdd)
                {
                    if (storedSongIds.Contains(song.Id))
                    {
                        continue;
                    }

                    playlist.Songs.Add(new PlaylistSongEntity
                    {
                        Playlist = playlist,
                        Song = song
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            var albumArtworkUrlMapping = await _albumStorageService.GetPresignedUrls(
                playlist.Songs.Select(x => x.Song.Album.S3ArtworkFilename),
                cancellationToken);

            return CommandResponse.FromEntity(playlist, albumArtworkUrlMapping, request.UserRegion);
        }
    }
}