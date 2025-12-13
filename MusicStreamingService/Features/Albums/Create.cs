using System.IO.Compression;
using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Binders;
using MusicStreamingService.Commands;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Extensions;
using MusicStreamingService.Features.Songs;
using MusicStreamingService.Features.Users;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Albums;

[ApiController]
public sealed class Create : ControllerBase
{
    private readonly IMediator _mediator;

    public Create(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new album
    /// </summary>
    /// <param name="request">Album data</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/albums")]
    [Tags(RouteGroups.Albums)]
    [Authorize(Roles = Permissions.ManageAlbumsPermission)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<CommandResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle(
        [FromForm] Command.CommandBody request,
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
            public sealed record SongArtistCreationData
            {
                [JsonPropertyName("artistId")]
                public Guid ArtistId { get; init; }

                [JsonPropertyName("mainArtist")]
                public bool MainArtist { get; init; }
            }

            public sealed record SongCreationData
            {
                [JsonPropertyName("title")]
                public string Title { get; init; } = null!;

                [JsonPropertyName("explicit")]
                public bool Explicit { get; init; }

                [JsonPropertyName("isTitleTrack")]
                public bool IsTitleTrack { get; init; }

                [JsonPropertyName("albumPosition")]
                public long AlbumPosition { get; init; }

                [JsonPropertyName("genreIds")]
                public List<Guid> GenreIds { get; init; } = null!;

                [JsonPropertyName("zipFilename")]
                public string ZipFilename { get; init; } = null!;

                [JsonPropertyName("artists")]
                public List<SongArtistCreationData> Artists { get; init; } = null!;

                [JsonPropertyName("durationMs")]
                public long DurationMs { get; init; }
            }

            [JsonPropertyName("albumZip")]
            public IFormFile AlbumZip { get; init; } = null!;

            [JsonPropertyName("title")]
            public string Title { get; init; } = null!;

            [JsonPropertyName("description")]
            public string? Description { get; init; }

            [JsonPropertyName("releaseDate")]
            public DateOnly ReleaseDate { get; init; }

            [JsonPropertyName("artworkImage")]
            public IFormFile ArtworkImage { get; init; } = null!;

            [JsonPropertyName("allowedRegions")]
            public List<Guid> AllowedRegions { get; init; } = null!;

            [JsonPropertyName("songs")]
            [FromForm]
            [ModelBinder(BinderType = typeof(JsonFormBinder))]
            public List<SongCreationData> Songs { get; init; } = null!;
        }

        public CommandBody Body { get; init; } = null!;

        public Guid UserId { get; init; }

        public sealed class Validator : AbstractValidator<CommandBody>
        {
            public Validator()
            {
                RuleFor(x => x.Title).NotEmpty();
                RuleFor(x => x.AlbumZip.ContentType).Equal("application/zip");
                RuleFor(x => x.AlbumZip.Length).GreaterThan(0);
                RuleFor(x => x.ArtworkImage.ContentType).Matches("^image/(png|jpeg)$");
                RuleFor(x => x.ArtworkImage.Length).GreaterThan(0);
                RuleFor(x => x.Songs).NotEmpty();
                RuleFor(x => x.Description).NotEmpty().When(x => x.Description is not null);
                RuleFor(x => x.AllowedRegions).NotEmpty();
                RuleFor(x => x.AllowedRegions)
                    .Must(x => x.Distinct().Count() == x.Count)
                    .WithMessage("Allowed regions must be distinct.");
                RuleFor(x => x.ReleaseDate).LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));

                RuleForEach(b => b.Songs)
                    .ChildRules(s =>
                    {
                        s.RuleFor(x => x.Title).NotEmpty();
                        s.RuleFor(x => x.AlbumPosition).GreaterThanOrEqualTo(0);
                        s.RuleFor(x => x.GenreIds).NotEmpty();
                        s.RuleFor(x => x.ZipFilename).NotEmpty();
                        s.RuleFor(x => x.Artists).NotEmpty();
                        s.RuleFor(x => x.DurationMs).NotEmpty();

                        s.RuleFor(x => x.Artists)
                            .Must(x => x.Distinct().Count() == x.Count)
                            .WithMessage("Song artists must be distinct.");
                        s.RuleFor(x => x.GenreIds)
                            .Must(x => x.Distinct().Count() == x.Count)
                            .WithMessage("Song genres must be distinct.");
                    });

                RuleFor(x => x)
                    .Must(x => x.Songs.Count == x.Songs.Select(s => s.AlbumPosition).Distinct().Count())
                    .WithMessage("Album songs must have continuous album positions starting from 0.");
            }
        }
    }

    public sealed record CommandResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("title")]
        public string Title { get; init; } = null!;

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("likes")]
        public int Likes { get; init; }

        [JsonPropertyName("artist")]
        public AlbumCreator Artist { get; init; } = null!;

        [JsonPropertyName("releaseDate")]
        public DateTime ReleaseDate { get; init; }

        [JsonPropertyName("artworkUrl")]
        public string ArtworkUrl { get; init; } = null!;

        [JsonPropertyName("songs")]
        public List<ShortAlbumSongDto> Songs { get; init; } = null!;

        public static CommandResponse FromEntity(
            AlbumEntity album,
            string? artworkUrl) =>
            new CommandResponse
            {
                Id = album.Id,
                Title = album.Title,
                Description = album.Description,
                Likes = album.Likes,
                Artist = AlbumCreator.FromEntity(album.Artist),
                ReleaseDate = album.ReleaseDate,
                ArtworkUrl = artworkUrl ?? string.Empty,
                Songs = album.Songs.Select(ShortAlbumSongDto.FromEntity).ToList()
            };
    }

    public sealed class Handler : IRequestHandler<Command, Result<CommandResponse>>
    {
        private readonly MusicStreamingContext _context;
        private readonly ISongStorageService _songStorageService;
        private readonly IAlbumStorageService _albumStorageService;

        public Handler(
            MusicStreamingContext context,
            IAlbumStorageService albumStorageService,
            ISongStorageService songStorageService)
        {
            _context = context;
            _albumStorageService = albumStorageService;
            _songStorageService = songStorageService;
        }

        public async ValueTask<Result<CommandResponse>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            var requestBody = request.Body;

            var artistValidationResults = await GetValidateArtists(request, cancellationToken);
            if (artistValidationResults.IsError)
            {
                return artistValidationResults.Error();
            }

            var genresValidationResult = await GetValidateGenres(request, cancellationToken);
            if (genresValidationResult.IsError)
            {
                return genresValidationResult.Error();
            }

            var genres = genresValidationResult.Success();

            var regionsValidationResult = await GetValidateRegions(request, cancellationToken);
            if (regionsValidationResult.IsError)
            {
                return regionsValidationResult.Error();
            }

            var regions = regionsValidationResult.Success();

            await using var fileStream = requestBody.AlbumZip.OpenReadStream();
            using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read, false);

            var songFileValidationResult = ValidateSongFiles(request, zipArchive);
            if (songFileValidationResult.IsError)
            {
                return songFileValidationResult.Error();
            }

            var zipFileToArchiveEntryMapping = songFileValidationResult.Success();

            var albumCoverFileExtension = ContentTypeUtils.GetFileExtensionByContentType(
                requestBody.ArtworkImage.ContentType);
            var albumCoverFileName = $"{Guid.NewGuid()}.{albumCoverFileExtension}";

            await using var albumCoverFileStream = requestBody.ArtworkImage.OpenReadStream();
            var albumArtworkUploadResult = await _albumStorageService.UploadAlbumArtwork(
                albumCoverFileName,
                requestBody.ArtworkImage.ContentType,
                albumCoverFileStream);

            if (albumArtworkUploadResult.IsError)
            {
                throw albumArtworkUploadResult.Error();
            }

            var albumArtworkLink = albumArtworkUploadResult.Success();

            var album = new AlbumEntity
            {
                Title = requestBody.Title,
                Description = requestBody.Description,
                ArtistId = request.UserId,
                ReleaseDate = requestBody.ReleaseDate.ToDateTime(new TimeOnly()).ToUniversalTime(),
                S3ArtworkFilename = albumCoverFileName,
                Songs = [],
            };

            await _context.Albums.AddAsync(album, cancellationToken);

            var songsToAdd = new List<SongEntity>();

            foreach (var songData in requestBody.Songs)
            {
                var mp3FileZipArchiveEntry = zipFileToArchiveEntryMapping[songData.ZipFilename];

                await using var mp3FileStream = mp3FileZipArchiveEntry.Open();
                await using var memoryMp3FileStream = new MemoryStream();
                await mp3FileStream.CopyToAsync(memoryMp3FileStream, cancellationToken);
                memoryMp3FileStream.Position = 0;

                var allowedSongRegions = regions
                    .Where(x => requestBody.AllowedRegions.Contains(x.Id))
                    .ToList();
                var songGenres = genres.Where(x => songData.GenreIds.Contains(x.Id)).ToList();
                var song = CreateSong(songData, allowedSongRegions, album, songGenres);
                
                await using var songStream = mp3FileZipArchiveEntry.Open();
                var songUploadResult = await _songStorageService.UploadSong(song.S3MediaFileName, memoryMp3FileStream);

                if (songUploadResult.IsError)
                {
                    return songUploadResult.Error();
                }

                album.Songs.Add(song);
                songsToAdd.Add(song);
            }

            await _context.AddRangeAsync(songsToAdd, cancellationToken);

            return CommandResponse.FromEntity(album, albumArtworkLink);
        }

        private static SongEntity CreateSong(
            Command.CommandBody.SongCreationData songData,
            List<RegionEntity> allowedRegions,
            AlbumEntity album,
            List<GenreEntity> genres)
        {
            var song = new SongEntity
            {
                Title = songData.Title,
                DurationMs = songData.DurationMs,
                S3MediaFileName = $"{Guid.NewGuid()}.mp3",
                Explicit = songData.Explicit,
                Artists = [],
                AllowedRegions = allowedRegions,
                Album = album,
                IsTitleTrack = songData.IsTitleTrack,
                AlbumPosition = songData.AlbumPosition,
                Genres = genres,
            };

            var songArtists = songData.Artists.Select(x => new SongArtistEntity
                {
                    ArtistId = x.ArtistId,
                    MainArtist = x.MainArtist,
                    Song = song
                }
            );

            song.Artists.AddRange(songArtists);
            return song;
        }


        private async Task<Result<List<GenreEntity>>> GetValidateGenres(
            Command request,
            CancellationToken cancellationToken)
        {
            var songGenres = request.Body.Songs
                .SelectMany(s => s.GenreIds)
                .Distinct()
                .ToList();

            var genres = await _context.Genres
                .Where(g => songGenres.Contains(g.Id))
                .ToListAsync(cancellationToken);

            if (genres.Count != songGenres.Count)
            {
                return new Exception("One or more genres are invalid.");
            }

            return genres;
        }

        private async Task<Result<List<UserEntity>>> GetValidateArtists(
            Command request,
            CancellationToken cancellationToken)
        {
            var songArtistIds = request.Body.Songs
                .SelectMany(s => s.Artists)
                .Select(a => a.ArtistId)
                .Distinct()
                .ToList();

            if (songArtistIds.All(x => x != request.UserId))
            {
                return new Exception("The album creator must be one of the song artists.");
            }

            var artists = await _context.Users
                .Include(u => u.Roles)
                .ThenInclude(u => u.Permissions)
                .Where(u =>
                    songArtistIds.Contains(u.Id) && !u.Disabled &&
                    u.Roles.Any(r =>
                        r.Permissions.Any(p => p.Title == Permissions.ManageAlbumsPermission)
                    )
                )
                .ToListAsync(cancellationToken);

            if (artists.Count != songArtistIds.Count)
            {
                return new Exception("One or more artists are invalid.");
            }

            return artists;
        }

        private async Task<Result<List<RegionEntity>>> GetValidateRegions(
            Command request,
            CancellationToken cancellationToken)
        {
            var regions = await _context.Regions
                .Where(r => request.Body.AllowedRegions.Contains(r.Id))
                .ToListAsync(cancellationToken);

            if (regions.Count != request.Body.AllowedRegions.Count)
            {
                return new Exception("One or more regions are invalid.");
            }

            return regions;
        }

        private Result<Dictionary<string, ZipArchiveEntry>> ValidateSongFiles(
            Command request,
            ZipArchive zipArchive)
        {
            var zipArchiveFileNames = zipArchive.Entries.Select(x => x.Name).ToList();
            if (zipArchiveFileNames.Any(x => x.Split(".").Last() != "mp3"))
            {
                return new Exception("All song files must be in mp3 format.");
            }

            var metadataFilenames = request.Body.Songs.Select(x => x.ZipFilename).ToList();

            if (metadataFilenames.Any(x => !zipArchiveFileNames.Contains(x)))
            {
                return new Exception("Song filenames in the zip do not match the provided song data.");
            }

            return zipArchive.Entries.ToDictionary(
                x => x.Name,
                x => x);
        }
    }
}