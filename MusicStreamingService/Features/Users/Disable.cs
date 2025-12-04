using System.IdentityModel.Tokens.Jwt;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Infrastructure.Result;

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
    [ProducesResponseType<Unit>( StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DisableHandler(
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Command
            {
                UserId = new Guid(
                    User.Claims
                        .Single(x => x.Type == JwtRegisteredClaimNames.Sid).Value
                ),
            },
            cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    public sealed record Command : IRequest<Result<Unit, Exception>>
    {
        public Guid UserId { get; init; }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<Unit, Exception>>
    {
        private readonly MusicStreamingContext _context;

        public Handler(
            MusicStreamingContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<Unit, Exception>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var user = await _context.Users.SingleOrDefaultAsync(
                x => x.Id == request.UserId && !x.Disabled,
                cancellationToken);
            if (user == null)
            {
                return new Exception("User not found or already disabled");
            }

            user.Disabled = true;
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}