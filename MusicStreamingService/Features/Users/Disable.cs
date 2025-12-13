using System.IdentityModel.Tokens.Jwt;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Extensions;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Users;

[ApiController]
public sealed class Disable : ControllerBase
{
    private readonly IMediator _mediator;

    public Disable(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Disable user's account
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("/api/v1/users")]
    [Tags(RouteGroups.Users)]
    [Authorize(Roles = Permissions.ManageUsersPermission)]
    [ProducesResponseType<Unit>(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DisableUser(
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Command
            {
                UserId = User.GetUserId(),
            },
            cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    public sealed record Command : IRequest<Result<Unit>>
    {
        public Guid UserId { get; init; }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly MusicStreamingContext _context;

        public Handler(
            MusicStreamingContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<Unit>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var user = await _context.Users.SingleOrDefaultAsync(
                x => x.Id == request.UserId,
                cancellationToken);
            if (user == null)
            {
                return new Exception("User not found");
            }
            
            if (user.Disabled)
            {
                return new Exception("User already disabled");
            }

            user.Disabled = true;
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}