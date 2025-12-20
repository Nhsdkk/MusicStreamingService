using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Common.Result;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Extensions;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Playlists;

[ApiController]
public sealed class Favorite : ControllerBase
{
    private readonly IMediator _mediator;

    public Favorite(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Add playlist to favorites
    /// </summary>
    /// <param name="request">Id of the playlist</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/playlists/favorite")]
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
                .AsNoTracking()
                .Include(x => x.Creator)
                .Include(x => x.LikedByUsers)
                .SingleOrDefaultAsync(x => x.Id == request.Body.Id, cancellationToken);

            if (playlist is null)
            {
                return new Exception("Playlist not found.");
            }

            if (playlist.AccessType is not PlaylistAccessType.Public && playlist.Creator.Id != request.UserId)
            {
                return new Exception("You do not have access to like this playlist.");
            }

            if (playlist.LikedByUsers.Any(x => x.Id == request.UserId))
            {
                return new Exception("Playlist is already liked by the user.");
            }

            await _context.PlaylistFavorites.AddAsync(new PlaylistFavoriteEntity
            {
                UserId = request.UserId,
                PlaylistId = request.Body.Id,
            }, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}