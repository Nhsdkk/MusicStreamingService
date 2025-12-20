using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Auth;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Features.Devices;
using MusicStreamingService.Features.Region;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.Password;
using MusicStreamingService.Common.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Users;

[ApiController]
public sealed class Login : ControllerBase
{
    private readonly IMediator _mediator;

    public Login(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Login user using username and password
    /// </summary>
    /// <param name="request">User's data</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/users/login")]
    [Tags(RouteGroups.Users)]
    [ProducesResponseType(typeof(CommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Exception), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginHandler(
        Command request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record Command : IRequest<Result<CommandResponse>>
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = null!;

        [JsonPropertyName("password")]
        public string Password { get; set; } = null!;

        [JsonPropertyName("deviceId")]
        public Guid? DeviceId { get; set; }

        [JsonPropertyName("deviceName")]
        public string DeviceName { get; set; } = null!;

        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Username).NotEmpty();
                RuleFor(x => x.Password).NotEmpty();
                RuleFor(x => x.DeviceName).NotEmpty();
            }
        }
    }

    public sealed record CommandResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("email")]
        public string Email { get; init; } = null!;

        [JsonPropertyName("fullName")]
        public string FullName { get; init; } = null!;

        [JsonPropertyName("birthDate")]
        public DateOnly BirthDate { get; init; }

        [JsonPropertyName("username")]
        public string Username { get; init; } = null!;

        [JsonPropertyName("region")]
        public RegionDto Region { get; init; } = null!;

        [JsonPropertyName("device")]
        public DeviceDto CurrentDevice { get; init; } = null!;

        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; init; } = null!;

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; init; } = null!;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; init; } = null!;
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
                .AsNoTracking()
                .Include(x => x.Region)
                .Include(x => x.Roles)
                .ThenInclude(x => x.Permissions)
                .Include(x => x.Devices.Where(device => device.Id == request.DeviceId))
                .SingleOrDefaultAsync(
                    x => x.Username == request.Username,
                    cancellationToken);

            if (user is null || user.Disabled)
            {
                return new Exception("Invalid credentials");
            }

            var passwordsMatch = _passwordService.Match(user.Password, request.Password);
            if (!passwordsMatch)
            {
                return new Exception("Invalid credentials");
            }

            if (user.Devices.Count == 0 && request.DeviceId.HasValue)
            {
                return new Exception("Can't use this device to login as it already belongs to another user");
            }

            if (user.Devices.Count == 0)
            {
                var newDevice = new DeviceEntity
                {
                    Title = request.DeviceName,
                    OwnerId = user.Id
                };

                user.Devices.Add(newDevice);

                await _context.AddAsync(newDevice, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var userClaims = UserClaimsCreator.FromEntity(user);
            var (accessToken, refreshToken) = _jwtService.GetPair(userClaims);

            return new CommandResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                Username = user.Username,
                Region = new RegionDto
                {
                    Id = user.Region.Id,
                    Title = user.Region.Title,
                },
                CurrentDevice = user.Devices.Select(x => new DeviceDto
                {
                    Id = x.Id,
                    Title = x.Title
                }).Single(),
                Permissions = user.GetPermissions().Select(x => x.Title).ToList(),
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}