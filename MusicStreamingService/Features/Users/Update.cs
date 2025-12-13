using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Auth;
using MusicStreamingService.Commands;
using MusicStreamingService.Data;
using MusicStreamingService.Extensions;
using MusicStreamingService.Features.Region;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.Password;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Infrastructure.Validations;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Users;

[ApiController]
public sealed class Update : ControllerBase
{
    private readonly IMediator _mediator;

    public Update(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Update user data
    /// </summary>
    /// <param name="request">New user data</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("/api/v1/users")]
    [Tags(RouteGroups.Users)]
    [Authorize(Roles = Permissions.ManageUsersPermission)]
    [ProducesResponseType<CommandResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(
        CommandBody request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new Command
            {
                Id = User.GetUserId(),
                Body = request
            },
            cancellationToken);

        return result.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record CommandBody
    {
        [JsonPropertyName("username")]
        public string? Username { get; init; }

        [JsonPropertyName("email")]
        public string? Email { get; init; }

        [JsonPropertyName("fullName")]
        public string? FullName { get; init; }

        [JsonPropertyName("password")]
        public string? Password { get; init; }

        [JsonPropertyName("birthDate")]
        public DateTime? BirthDate { get; init; }

        [JsonPropertyName("regionId")]
        public Guid? RegionId { get; init; }

        public sealed class Validator : AbstractValidator<CommandBody>
        {
            public Validator()
            {
                RuleFor(x => x.Username).NotEmpty().When(x => x.Username is not null);
                RuleFor(x => x.RegionId).NotEmpty().When(x => x.RegionId is not null);
                RuleFor(x => x.Password!).Password().When(x => x.Password is not null);
                RuleFor(x => x.Email).EmailAddress().When(x => x.Email is not null);
                RuleFor(x => x.BirthDate!.Value).Before(DateTime.UtcNow).When(x => x.BirthDate is not null);
            }
        }
    }

    public sealed record Command : ITransactionWrappedCommand<Result<CommandResponse>>
    {
        public Guid Id { get; init; }

        public CommandBody Body { get; init; } = null!;
    }

    public sealed record CommandResponse
    {
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
        public ShortRegionDto Region { get; set; } = null!;

        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; set; } = null!;

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = null!;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = null!;
    }

    internal sealed class Handler : IRequestHandler<Command, Result<CommandResponse>>
    {
        private readonly MusicStreamingContext _context;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService<UserClaims> _jwtService;

        public Handler(
            MusicStreamingContext context,
            IPasswordService passwordService,
            IJwtService<UserClaims> jwtService)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        public async ValueTask<Result<CommandResponse>> Handle(
            Command request,
            CancellationToken cancellationToken)
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
                return new Exception("User account is disabled");
            }

            var newData = request.Body;

            if (newData.Username is not null)
            {
                var usernameExists =
                    await _context.Users.AnyAsync(x => x.Username == newData.Username && x.Id != request.Id,
                        cancellationToken);

                if (usernameExists)
                {
                    return new Exception("Username is already taken");
                }

                user.Username = newData.Username;
            }

            if (newData.Email is not null)
            {
                var emailExists =
                    await _context.Users.AnyAsync(x => x.Email == newData.Email && x.Id != request.Id,
                        cancellationToken);

                if (emailExists)
                {
                    return new Exception("User with the same email already exists");
                }

                user.Email = newData.Email;
            }

            user.BirthDate = newData.BirthDate?.ToUniversalTime() ?? user.BirthDate;
            user.FullName = newData.FullName ?? user.FullName;

            if (newData.Password is not null)
            {
                user.Password = _passwordService.Encode(newData.Password);
            }

            if (newData.RegionId is not null)
            {
                var region = await _context.Regions.FindAsync([newData.RegionId], cancellationToken: cancellationToken);
                if (region is null)
                {
                    return new Exception($"Can't find region");
                }

                user.RegionId = newData.RegionId.Value;
            }

            await _context.SaveChangesAsync(cancellationToken);

            var claims = UserClaimsCreator.FromEntity(user);
            var (accessToken, refreshToken) = _jwtService.GetPair(claims);

            return new CommandResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                Username = user.Username,
                Region = new ShortRegionDto
                {
                    Id = user.Region.Id,
                    Title = user.Region.Title
                },
                Permissions = user.GetPermissions().Select(x => x.Title).ToList(),
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}