using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Extensions;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.Result;

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
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
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

    internal sealed record Query : IRequest<Result<Response, Exception>>
    {
        public Guid Id { get; init; }
    }

    internal sealed record Response
    {
        internal sealed record RegionDto
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
        public DateTime BirthDate { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = null!;

        [JsonPropertyName("region")]
        public RegionDto Region { get; set; } = null!;

        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; set; } = null!;
    }

    internal sealed class Handler : IRequestHandler<Query, Result<Response, Exception>>
    {
        private readonly MusicStreamingContext _context;

        public Handler(MusicStreamingContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<Response, Exception>> Handle(Query request, CancellationToken cancellationToken)
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
            
            return new Response
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                Username = user.Username,
                Region = new Response.RegionDto
                {
                    Id = user.Region.Id,
                    Title = user.Region.Title
                },
                Permissions = user.GetPermissions().Select(x => x.Title).ToList()
            };
        }
    }
}