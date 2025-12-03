using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.Password;
using MusicStreamingService.Infrastructure.Result;

namespace MusicStreamingService.Features.Users;

[ApiController]
public sealed class Register : ControllerBase
{
    private const string UserRoleName = "mss.user";
    private const string ArtistRoleName = "mss.artist";

    private readonly IMediator _mediator;

    public Register(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register new user
    /// </summary>
    /// <param name="command">User data to process registration</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/users/register")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Exception), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterUser(
        CommandDto command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record CommandDto : IRequest<Result<ResponseDto, Exception>>
    {
        public enum AccountRegisterRole
        {
            User,
            Artist
        }

        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = null!;

        [JsonPropertyName("birthDate")]
        public DateTime BirthDate { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = null!;

        [JsonPropertyName("password")]
        public string Password { get; set; } = null!;

        [JsonPropertyName("regionId")]
        public Guid RegionId { get; set; }

        [JsonPropertyName("deviceName")]
        public string DeviceName { get; set; } = null!;

        [JsonPropertyName("accountRole")]
        public AccountRegisterRole Role { get; set; }
        
        public sealed class Validator : AbstractValidator<CommandDto>
        {
            public Validator()
            {
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.Password).MinimumLength(10);
                RuleFor(x => x.Username).MinimumLength(7);
                RuleFor(x => x.FullName).MinimumLength(10);
                RuleFor(x => x.BirthDate)
                    .Must(x => x.ToUniversalTime() <= DateTime.UtcNow);
                RuleFor(x => x.DeviceName).NotEmpty();
            }
        }
    }

    public sealed record ResponseDto
    {
        public sealed record RegionDto
        {
            [JsonPropertyName("id")]
            public Guid Id { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; } = null!;
        }

        public sealed record DeviceDto
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

        [JsonPropertyName("device")]
        public List<DeviceDto> Devices { get; set; } = null!;

        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; set; } = null!;

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = null!;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = null!;
    }

    public sealed class Handler : IRequestHandler<CommandDto, Result<ResponseDto, Exception>>
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

        public async ValueTask<Result<ResponseDto, Exception>> Handle(CommandDto request,
            CancellationToken cancellationToken)
        {
            var userWithSameEmailExists = await _context.Users.AnyAsync(
                x => x.Email == request.Email,
                cancellationToken);
            if (userWithSameEmailExists)
            {
                return new Exception("User with the same email already exists");
            }

            var userWithSameUsernameExists = await _context.Users.AnyAsync(
                x => x.Username == request.Username,
                cancellationToken);
            if (userWithSameUsernameExists)
            {
                return new Exception("User with the same email already exists");
            }

            var region = await _context.Regions.FindAsync([request.RegionId], cancellationToken: cancellationToken);
            if (region is null)
            {
                return new Exception($"Can't find region with id {request.RegionId}");
            }

            var requestedRoleName = request.Role switch
            {
                CommandDto.AccountRegisterRole.Artist => ArtistRoleName,
                CommandDto.AccountRegisterRole.User => UserRoleName,
                _ => throw new ArgumentOutOfRangeException(nameof(CommandDto.AccountRegisterRole))
            };

            var role = await _context.Roles
                .Include(x => x.Permissions)
                .SingleOrDefaultAsync(x => x.Title == requestedRoleName, cancellationToken);
            if (role is null)
            {
                throw new Exception($"Can't find role with name {requestedRoleName}");
            }

            var device = new DeviceEntity { Title = request.DeviceName };

            var user = new UserEntity
            {
                Email = request.Email,
                Username = request.Username,
                BirthDate = request.BirthDate.ToUniversalTime(),
                Devices = [device],
                FullName = request.FullName,
                RegionId = request.RegionId,
                Password = _passwordService.Encode(request.Password),
                Roles = [role]
            };

            await _context.Devices.AddAsync(device, cancellationToken);
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var (accessToken, refreshToken) = _jwtService.GetPair(new UserClaims(user));
            return new ResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                Username = user.Username,
                Region = new ResponseDto.RegionDto
                {
                    Id = user.Region.Id,
                    Title = user.Region.Title
                },
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Devices = user.Devices
                    .Select(x => new ResponseDto.DeviceDto
                    {
                        Id = x.Id,
                        Title = x.Title
                    }).ToList(),
                Permissions = user.GetPermissions().Select(x => x.Title).ToList(),
            };
        }
    }
}