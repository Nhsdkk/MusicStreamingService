using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Commands;
using MusicStreamingService.Data;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Albums;

[ApiController]
public sealed class Delete : ControllerBase
{
    private readonly IMediator _mediator;

    public Delete(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    /// <summary>
    /// Delete an album with all related songs
    /// </summary>
    /// <param name="request">Id of the album</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("/api/v1/albums")]
    [Tags(RouteGroups.Albums)]
    [Authorize(Roles = Permissions.ManageAlbumsPermission)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAlbum(
        [FromBody] Command.CommandBody request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Command
            {
                Body = request
            },
            cancellationToken);
        
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    public sealed record Command : ITransactionWrappedCommand<Result<Unit>>
    {
        public sealed record CommandBody
        {
            [JsonPropertyName("albumId")]
            public Guid AlbumId { get; init; }
        }
        
        public Guid UserId { get; init; }
        
        public CommandBody Body { get; init; } = null!;
        
        public sealed class Validator : AbstractValidator<CommandBody>
        {
            public Validator()
            {
                RuleFor(x => x.AlbumId).NotEmpty();
            }
        }
    }
    
    public sealed class Handler : IRequestHandler<Command, Result<Unit>>
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

        public async ValueTask<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var albumId = request.Body.AlbumId;
            
            var album = await _context.Albums
                .Include(x => x.Songs)
                .SingleOrDefaultAsync(x => x.Id == albumId, cancellationToken);

            if (album is null)
            {
                return new Exception("Album not found");
            }
            
            if (album.ArtistId != request.UserId)
            {
                return new Exception("You do not have permission to delete this album");
            }

            var albumCoverFileName = album.S3ArtworkFilename;
            var songsFileNames = album.Songs.Select(x => x.S3MediaFileName).ToList();

            // TODO: Consider marking files for deletion and deleting them later in a background job
            await _albumStorageService.DeleteAlbumArtwork(albumCoverFileName);
            await _songStorageService.DeleteSongs(songsFileNames);
            
            _context.Albums.Remove(album);
            return Unit.Value;
        }
    }
}