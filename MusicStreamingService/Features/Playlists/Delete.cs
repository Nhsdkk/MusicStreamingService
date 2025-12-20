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
public sealed class Delete : ControllerBase
{
    private readonly IMediator _mediator;

    public Delete(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Delete playlist by id
    /// </summary>
    /// <param name="request">Playlist id</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("/api/v1/playlists")]
    [Tags(RouteGroups.Playlists)]
    [Authorize(Roles = Permissions.ManagePlaylistsPermission)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeletePlaylist(
        [FromBody] Command.CommandBody request,
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
        private readonly MusicStreamingContext _dataContext;

        public Handler(MusicStreamingContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async ValueTask<Result<Unit>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var playlist = await _dataContext.Playlists
                .Include(x => x.Creator)
                .SingleOrDefaultAsync(x => x.Id == request.Body.Id, cancellationToken);

            if (playlist == null)
            {
                return new Exception("Playlist not found");
            }

            if (playlist.Creator.Id != request.UserId)
            {
                return new Exception("You do not have permission to delete this playlist");
            }

            _dataContext.Playlists.Remove(playlist);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}