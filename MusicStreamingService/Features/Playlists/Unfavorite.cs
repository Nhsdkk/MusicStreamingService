using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Extensions;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Playlists;

[ApiController]
public sealed class Unfavorite : ControllerBase
{
    private readonly IMediator _mediator;

    public Unfavorite(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Remove playlist from favorites
    /// </summary>
    /// <param name="request">Id of the playlist</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/playlists/unfavorite")]
    [Tags(RouteGroups.Playlists)]
    [Authorize(Roles = Permissions.FavoritePlaylistsPermission)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FavoritePlaylist(
        Command.CommandBody request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Command
            { 
                Body = request,
                UserId = User.GetUserId(),
            },
            cancellationToken);

        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    public sealed record Command : IRequest<Result<Unit>>
    {
        public sealed record CommandBody
        {
            [JsonPropertyName("id")]
            public Guid Id { get; init; }

            public sealed class Validator : AbstractValidator<CommandBody>
            {
                public Validator()
                {
                    RuleFor(x => x.Id).NotEmpty();
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
            var playlist = await _context.Playlists
                .Include(x => x.Creator)
                .Include(x => x.LikedByUsers)
                .SingleOrDefaultAsync(x => x.Id == request.Body.Id, cancellationToken);

            if (playlist is null)
            {
                return new Exception("Playlist not found.");
            }

            var favorite = playlist.LikedByUsers.SingleOrDefault(x => x.Id != request.UserId); 
            if (favorite is null)
            {
                return new Exception("Playlist is not in favorites.");
            }

            playlist.LikedByUsers.Remove(favorite);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}