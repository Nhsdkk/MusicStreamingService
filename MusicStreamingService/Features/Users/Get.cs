using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Result;
using OneOf;
using OneOf.Types;

namespace MusicStreamingService.Features.Users;

[ApiController]
public sealed class Get : ControllerBase
{
    private readonly IMediator _mediator;

    public Get(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get user data by his id
    /// </summary>
    /// <param name="id">User's id</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/users/")]
    [ProducesResponseType(typeof(UserEntity), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUser(
        [FromQuery] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Query
            {
                Id = id
            },
            cancellationToken);
        
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    internal sealed record Query : IRequest<Result<UserEntity, string>>
    {
        public Guid Id { get; init; }
    }

    internal sealed class Handler : IRequestHandler<Query, Result<UserEntity, string>>
    {
        private readonly MusicStreamingContext _context;

        public Handler(MusicStreamingContext context)
        {
            _context = context;
        }
        
        public async ValueTask<Result<UserEntity, string>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(x => x.Region)
                .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            
            if (user is null)
            {
                return "User not found";
            }
            
            return user;
        }
    }
}