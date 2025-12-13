using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Users;

[ApiController]
public sealed class RefreshToken : ControllerBase
{
    private readonly IMediator _mediator;

    public RefreshToken(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="command">Refresh token</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/users/refresh-token")]
    [Tags(RouteGroups.Users)]
    [ProducesResponseType(typeof(CommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Exception), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh(
        [FromBody] Command command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record Command : IRequest<Result<CommandResponse>>
    {
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; init; } = null!;

        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.RefreshToken).NotEmpty();
            }
        }
    }

    public sealed record CommandResponse
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; init; } = null!;
    }

    public sealed class Handler : IRequestHandler<Command, Result<CommandResponse>>
    {
        private readonly IJwtService<UserClaims> _jwtService;

        public Handler(
            IJwtService<UserClaims> jwtService)
        {
            _jwtService = jwtService;
        }

        public async ValueTask<Result<CommandResponse>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var result = await _jwtService.RefreshAccessToken(request.RefreshToken, cancellationToken);
            return result.Match<Result<CommandResponse>>(
                accessToken => new CommandResponse
                {
                    AccessToken = accessToken
                },
                exception => exception);
        }
    }
}