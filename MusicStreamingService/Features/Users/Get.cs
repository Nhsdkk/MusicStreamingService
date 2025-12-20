using System.Text.Json.Serialization;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Extensions;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Common.Result;
using MusicStreamingService.Openapi;

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
    /// Get full user's data
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/users/")]
    [Tags(RouteGroups.Users)]
    [ProducesResponseType(typeof(CommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Exception), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = Permissions.ViewUsersPermission)]
    public async Task<IActionResult> GetUser(
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Query
            {
                Id = User.GetUserId()
            },
            cancellationToken);

        return result.Match<IActionResult>(Ok, BadRequest);
    }

    internal sealed record Query : IRequest<Result<CommandResponse>>
    {
        public Guid Id { get; init; }
    }

    public sealed record CommandResponse
    {
        public sealed record RegionDto
        {
            [JsonPropertyName("id")]
            public Guid Id { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; } = null!;
        }

        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = null!;

        [JsonPropertyName("birthDate")]
        public DateOnly BirthDate { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = null!;

        [JsonPropertyName("region")]
        public RegionDto Region { get; set; } = null!;

        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; set; } = null!;
    }

    internal sealed class Handler : IRequestHandler<Query, Result<CommandResponse>>
    {
        private readonly MusicStreamingContext _context;

        public Handler(MusicStreamingContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<CommandResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(x => x.Region)
                .Include(x => x.Roles)
                .ThenInclude(x => x.Permissions)
                .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (user is null)
            {
                return new Exception("User not found");
            }

            if (user.Disabled)
            {
                return new Exception("User is disabled");
            }
            
            return new CommandResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                Username = user.Username,
                Region = new CommandResponse.RegionDto
                {
                    Id = user.Region.Id,
                    Title = user.Region.Title
                },
                Permissions = user.GetPermissions().Select(x => x.Title).ToList()
            };
        }
    }
}