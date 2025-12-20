using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Extensions;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Common.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Songs;

[ApiController]
public sealed class Unfavorite : ControllerBase
{
    private readonly IMediator _mediator;

    public Unfavorite(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Unfavorite a song
    /// </summary>
    /// <param name="request">Id of the song</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/songs/unfavorite")]
    [Tags(RouteGroups.Songs)]
    [Authorize(Roles = Permissions.FavoriteSongsPermission)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnfavoriteSong(
        [FromBody] Command.CommandBody request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new Command
        {
            Body = request,
            UserId = User.GetUserId()
        }, cancellationToken);

        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    public sealed record Command : IRequest<Result<Unit>>
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
    }

    public sealed class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly MusicStreamingContext _context;

        public Handler(MusicStreamingContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<Unit>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var songId = request.Body.SongId;
            var userId = request.UserId;

            var songFavorite = await _context.SongFavorites
                .SingleOrDefaultAsync(
                    fs => fs.SongId == songId && fs.UserId == userId,
                    cancellationToken);

            if (songFavorite is null)
            {
                return new Exception("Song is not favorited");
            }

            _context.Remove(songFavorite);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}