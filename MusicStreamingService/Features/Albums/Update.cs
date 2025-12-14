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
public sealed class Update : ControllerBase
{
    private readonly IMediator _mediator;

    public Update(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Update album data
    /// </summary>
    /// <param name="request">Album data</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("api/v1/albums")]
    [Authorize(Roles = Permissions.ManageAlbumsPermission)]
    [Tags(RouteGroups.Albums)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<CommandResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAlbum(
        [FromForm] Command.CommandBody request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Command
            {
                UserId = User.GetUserId(),
                Body = request
            }, cancellationToken);
        
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record Command : ITransactionWrappedCommand<Result<CommandResponse>>
    {
        public sealed record CommandBody
        {
            public sealed record SongOrdering
            {
                [JsonPropertyName("songId")]
                public Guid SongId { get; init; }

                [JsonPropertyName("order")]
                public int Order { get; init; }
                
                [JsonPropertyName("isTitleTrack")]
                public bool IsTitleTrack { get; init; }
            }
            
            [JsonPropertyName("id")]
            public Guid Id { get; init; }

            [JsonPropertyName("title")]
            public string? Title { get; init; }

            [JsonPropertyName("description")]
            public string? Description { get; init; }

            [JsonPropertyName("descriptionFilled")]
            public bool DescriptionFilled { get; init; }

            [JsonPropertyName("releaseDate")]
            public DateOnly? ReleaseDate { get; init; }

            [JsonPropertyName("songOrderings")]
            [FromForm]
            [ModelBinder(BinderType = typeof(JsonFormBinder))]
            public List<SongOrdering>? SongOrderings { get; init; }
            
            [JsonPropertyName("artworkFile")]
            public IFormFile? ArtworkFile { get; init; }
        }

        public Guid UserId { get; init; }

        public CommandBody Body { get; init; } = null!;

        public sealed class Validator : AbstractValidator<CommandBody>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Title)
                    .NotEmpty()
                    .MaximumLength(255)
                    .When(x => x.Title is not null);

                RuleFor(x => x.Description)
                    .NotEmpty()
                    .When(x => x.Description is not null);

                RuleFor(x => x.ReleaseDate)
                    .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                    .When(x => x.ReleaseDate is not null);

                RuleForEach(x => x.SongOrderings)
                    .ChildRules(songOrdering =>
                    {
                        songOrdering.RuleFor(s => s.Order).GreaterThanOrEqualTo(0);
                        songOrdering.RuleFor(s => s.SongId).NotEmpty();
                    })
                    .When(x => x.SongOrderings is not null);

                RuleFor(x => x.SongOrderings)
                    .Must(s =>
                        s!.Select(x => x.Order)
                            .ToHashSet()
                            .SetEquals(s!.Select((_, idx) => idx)))
                    .When(x => x.SongOrderings is not null)
                    .WithMessage("Song orderings must contain all orders from 0 to n-1 without gaps.");
                
                RuleFor(x => x.SongOrderings)
                    .Must(s => 
                        s!.Select(x => x.SongId)
                            .ToHashSet()
                            .Distinct()
                            .Count() == s!.Count)
                    .When(x => x.SongOrderings is not null)
                    .WithMessage("Song orderings must not contain duplicate song IDs.");
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
        public long Likes { get; init; }

        [JsonPropertyName("artist")]
        public ShortAlbumCreatorDto Artist { get; init; } = null!;

        [JsonPropertyName("releaseDate")]
        public DateOnly ReleaseDate { get; init; }

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
                Artist = ShortAlbumCreatorDto.FromEntity(album.Artist),
                ReleaseDate = album.ReleaseDate,
                ArtworkUrl = artworkUrl ?? string.Empty,
                Songs = album.Songs.Select(ShortAlbumSongDto.FromEntity).OrderBy(x => x.AlbumPosition).ToList()
            };
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

        public async ValueTask<Result<CommandResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            var body = request.Body;
            var album = await _context.Albums
                .Include(x => x.Artist)
                .Include(x => x.Songs)
                .ThenInclude(x => x.Artists)
                .ThenInclude(x => x.Artist)
                .Include(x => x.Songs)
                .ThenInclude(x => x.AllowedRegions) 
                .Include(x => x.Songs)
                .ThenInclude(x => x.Genres)
                .SingleOrDefaultAsync(x => x.Id == body.Id, cancellationToken);

            if (album is null)
            {
                return new Exception("Album not found");
            }

            if (album.ArtistId != request.UserId)
            {
                return new Exception("User is not the creator of the album");
            }
            
            album.Title = body.Title ?? album.Title;
            album.Description = body.DescriptionFilled ? body.Description : album.Description;
            album.ReleaseDate = body.ReleaseDate ?? album.ReleaseDate;

            if (body.SongOrderings is not null)
            {
                var songCount = album.Songs.Count;

                if (songCount != body.SongOrderings.Count)
                {
                    return new Exception("Song orderings count does not match the number of songs in the album");
                }
                
                var dbSongIdsHashSet = album.Songs
                    .Select(x => x.Id)
                    .ToHashSet();
                var requestSongIdsHashSet = body.SongOrderings
                    .Select(x => x.SongId)
                    .ToHashSet();

                if (!dbSongIdsHashSet.SetEquals(requestSongIdsHashSet))
                {
                    return new Exception("Some song ids not found in the album");
                }

                var songOrderMapping = body.SongOrderings.ToDictionary(x => x.SongId, x => x.Order);

                foreach (var song in album.Songs)
                {
                    var position = songOrderMapping[song.Id];
                    song.AlbumPosition = position;
                }
            }
            
            string? presignedArtworkUrl = null;
                
            if (body.ArtworkFile is not null)
            {
                var fileExtension = ContentTypeUtils.GetFileExtensionByContentType(body.ArtworkFile.ContentType);
                var oldFilename = album.S3ArtworkFilename;
                var newFilename = $"{Guid.NewGuid()}.{fileExtension}";
                    
                var uploadResult = await _albumStorageService.UploadAlbumArtwork( 
                    newFilename,
                    body.ArtworkFile.ContentType,
                    body.ArtworkFile.OpenReadStream());

                if (uploadResult.IsError)
                {
                    throw new Exception("Failed to upload album artwork");
                }

                presignedArtworkUrl = uploadResult.Success();
                album.S3ArtworkFilename = newFilename;
                    
                // TODO: Consider marking files for deletion and deleting them later in a background job
                await _albumStorageService.DeleteAlbumArtwork(oldFilename);
            }
                
            if (presignedArtworkUrl is null)
            {
                var getUrlResult = await _albumStorageService.GetPresignedUrl(album.S3ArtworkFilename);
                if (getUrlResult.IsError)
                {
                    throw new Exception("Failed to get album artwork URL");
                }

                presignedArtworkUrl = getUrlResult.Success();
            }

            return CommandResponse.FromEntity(album, presignedArtworkUrl);
        }
    }
}