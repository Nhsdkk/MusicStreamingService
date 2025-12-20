using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Extensions;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Common.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Albums;

[ApiController]
public sealed class Favorite : ControllerBase
{
    private readonly IMediator _mediator;

    public Favorite(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Add album to favorites
    /// </summary>
    /// <param name="request">Id of the album</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/albums/favorite")]
    [Tags(RouteGroups.Albums)]
    [Authorize(Roles = Permissions.FavoriteAlbumsPermission)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FavoriteAlbum(
        [FromBody] Command.CommandBody request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Command
            {
                UserId = User.GetUserId(),
                Body = request
            },
            cancellationToken);

        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    public sealed record Command : IRequest<Result<Unit>>
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

        public Handler(MusicStreamingContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var albumId = request.Body.AlbumId;
            var album = await _context.Albums
                .SingleOrDefaultAsync(x => x.Id == albumId, cancellationToken);
            if (album is null)
            {
                return new Exception("Album not found");
            }

            var albumInFavorites = await _context.AlbumFavorites.AnyAsync(
                x => x.AlbumId == albumId && x.UserId == request.UserId,
                cancellationToken);

            if (albumInFavorites)
            {
                return new Exception("Album already in favorites");
            }

            await _context.AlbumFavorites.AddAsync(
                new AlbumFavoriteEntity
                {
                    UserId = request.UserId,
                    AlbumId = albumId
                },
                cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}