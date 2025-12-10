using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Extensions;
using MusicStreamingService.Features.Users;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Songs;

[ApiController]
public sealed class Favorite : ControllerBase
{
    private readonly IMediator _mediator;

    public Favorite(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Add song to favorites
    /// </summary>
    /// <param name="request">Id of the song</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/songs/favorite")]
    [Tags(RouteGroups.Songs)]
    [Authorize(Roles = Permissions.FavoriteSongsPermission)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FavoriteSong(
        [FromBody] Command.CommandBody request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new Command
        {
            Body = request,
            UserId = User.GetUserId(),
            Age = User.GetUserAge(),
        }, cancellationToken);

        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    public sealed record Command : IRequest<Result<Unit, Exception>>
    {
        public sealed record CommandBody
        {
            [JsonPropertyName("songId")]
            public Guid SongId { get; init; }

            public sealed class Validator : AbstractValidator<CommandBody>
            {
                public Validator()
                {
                    RuleFor(x => x.SongId).NotEmpty();
                }
            }
        }

        public CommandBody Body { get; init; } = null!;

        public Guid UserId { get; init; }
        
        public int Age { get; init; }
    }

    public sealed class Handler : IRequestHandler<Command, Result<Unit, Exception>>
    {
        private readonly MusicStreamingContext _context;

        public Handler(MusicStreamingContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<Unit, Exception>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var songId = request.Body.SongId;
            var userId = request.UserId;

            var song = await _context.Songs
                .SingleOrDefaultAsync(s => s.Id == songId, cancellationToken);

            if (song == null)
            {
                return new Exception("Song not found");
            }

            if (song.Explicit && request.Age < UserConstants.AdultLegalAge)
            {
                return new Exception($"Users under {UserConstants.AdultLegalAge} years old are not allowed to favorite explicit songs");
            }

            var alreadyFavorite = await _context.SongFavorites
                .AnyAsync(
                    fs => fs.SongId == songId && fs.UserId == userId,
                    cancellationToken);

            if (alreadyFavorite)
            {
                return new Exception("Song is already in favorites");
            }

            var songFavorite = new SongFavoriteEntity
            {
                SongId = songId,
                UserId = userId,
            };
            await _context.AddAsync(songFavorite, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}