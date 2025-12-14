using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.Result;

namespace MusicStreamingService.Features.Albums;

[ApiController]
public sealed class Unfavorite : ControllerBase
{
    private readonly IMediator _mediator;

    public Unfavorite(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Remove album from favorites
    /// </summary>
    /// <param name="albumId">Id of the album</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/albums/unfavorite")]
    [Authorize(Roles = Permissions.FavoriteAlbumsPermission)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnfavoriteAlbum(
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
            var albumFavoriteEntity = await _context.AlbumFavorites.SingleOrDefaultAsync(
                x => x.AlbumId == request.AlbumId && x.UserId == request.UserId,
                cancellationToken);

            if (albumFavoriteEntity is null)
            {
                return new Exception("Album is not in favorites");
            }

            _context.AlbumFavorites.Remove(albumFavoriteEntity);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}