using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Extensions;
using MusicStreamingService.Features.Songs;
using MusicStreamingService.Features.Users;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Common.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Albums;

[ApiController]
public sealed class Get : ControllerBase
{
    private readonly IMediator _mediator;

    public Get(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("/api/v1/albums")]
    [Tags(RouteGroups.Albums)]
    [Authorize(Roles = Permissions.ViewAlbumsPermission)]
    [ProducesResponseType<CommandResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAlbum(
        [FromQuery] Command.CommandBody request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Command
            {
                Body = request,
                UserAge = User.GetUserAge(),
                UserRegion = User.GetUserRegion(),
            },
            cancellationToken);
        
        return result.Match<IActionResult>(
            albumDto => Ok(albumDto),
            BadRequest);
    }

    public sealed record Command : IRequest<Result<CommandResponse>>
    {
        public sealed record CommandBody
        {
            [JsonPropertyName("id")]
            public Guid Id { get; init; }
        
            [JsonPropertyName("allowExplicit")]
            public bool AllowExplicit { get; init; } = false;
        }
        
        public CommandBody Body { get; init; } = null!;
        
        public int UserAge { get; init; } = 0;
        
        public RegionClaim UserRegion { get; init; } = null!;
        
        public sealed class Validator : AbstractValidator<CommandBody>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
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
        public ShortUserDto Artist { get; init; } = null!;
        
        [JsonPropertyName("releaseDate")]
        public DateOnly ReleaseDate { get; init; }
        
        [JsonPropertyName("artworkUrl")]
        public string ArtworkUrl { get; init; } = null!;
        
        [JsonPropertyName("songs")]
        public List<ShortAlbumSongDto> Songs { get; init; } = null!;
        
        public static CommandResponse FromEntity(
            AlbumEntity album,
            string artworkUrl,
            RegionClaim userRegion) =>
            new CommandResponse
            {
                Id = album.Id,
                Title = album.Title,
                Description = album.Description,
                Likes = album.Likes,
                Artist = ShortUserDto.FromEntity(album.Artist),
                ReleaseDate = album.ReleaseDate,
                ArtworkUrl = artworkUrl,
                Songs = album.Songs
                    .Select(x => ShortAlbumSongDto.FromEntity(x, userRegion))
                    .OrderBy(x => x.AlbumPosition)
                    .ToList()
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
            if (request.UserAge < UserConstants.AdultLegalAge && request.Body.AllowExplicit)
            {
                return new Exception("Explicit content is not allowed for underage users");
            }
            
            var album = await _context.Albums
                .Include(x => x.Songs)
                .ThenInclude(x => x.Genres)
                .Include(x => x.Songs)
                .ThenInclude(x => x.AllowedRegions)
                .Include(x => x.Songs)
                .ThenInclude(x => x.Artists)
                .ThenInclude(x => x.Artist)
                .SingleOrDefaultAsync(x => x.Id == request.Body.Id, cancellationToken);

            if (album is null)
            {
                return new Exception("Album not found");
            }
            
            var artworkUrlResult = await _albumStorageService.GetPresignedUrl(album.S3ArtworkFilename, cancellationToken);

            if (artworkUrlResult.IsError)
            {
                throw artworkUrlResult.Error();
            }
            
            return CommandResponse.FromEntity(
                album, 
                artworkUrlResult.Success(),
                request.UserRegion);
        }
        
        
    }
}