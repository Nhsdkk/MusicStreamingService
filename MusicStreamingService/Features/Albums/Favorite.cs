using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.Result;

namespace MusicStreamingService.Features.Albums;

[ApiController]
public class Favorite : ControllerBase
{
    private readonly IMediator _mediator;

    public Favorite(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Add album to favorites
    /// </summary>
    /// <param name="albumId">Id of the album</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/albums/favorite")]
    [Authorize(Roles = Permissions.FavoriteAlbumsPermission)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FavoriteAlbum(
        [FromBody] Guid albumId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Command
            {
                UserId = Guid.Parse(User.Identity!.Name!),
                AlbumId = albumId
            },
            cancellationToken);

        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    public sealed record Command : IRequest<Result<Unit>>
    {
        public Guid UserId { get; init; }

        public Guid AlbumId { get; init; }
    }

    public sealed class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly MusicStreamingContext _context;

        public Handler(MusicStreamingContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var album = await _context.Albums
                .SingleOrDefaultAsync(x => x.Id == request.AlbumId, cancellationToken);
            if (album is null)
            {
                return new Exception("Album not found");
            }

            var albumInFavorites = await _context.AlbumFavorites.AnyAsync(
                x => x.AlbumId == request.AlbumId && x.UserId == request.UserId,
                cancellationToken);

            if (albumInFavorites)
            {
                return new Exception("Album already in favorites");
            }

            await _context.AlbumFavorites.AddAsync(
                new AlbumFavoriteEntity
                {
                    UserId = request.UserId,
                    AlbumId = request.AlbumId
                },
                cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}