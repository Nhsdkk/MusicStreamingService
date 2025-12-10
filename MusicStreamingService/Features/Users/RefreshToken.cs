using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Auth;
using MusicStreamingService.Data;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.Result;

namespace MusicStreamingService.Features.Users;

[ApiController]
public sealed class RefreshToken : ControllerBase
{
    private readonly IMediator _mediator;

    public RefreshToken(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/api/v1/users/refresh-token")]
    public async Task<IActionResult> Refresh(
        [FromBody] Command command)
    {
        var result =  await _mediator.Send(command);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record Command : IRequest<Result<CommandResponse, Exception>>
    {
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = null!;
    }
    
    public sealed record CommandResponse
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; init; } = null!;
    }

    public sealed class Handler : IRequestHandler<Command, Result<CommandResponse, Exception>>
    {
        private readonly IJwtService<UserClaims> _jwtService;

        public Handler(
            IJwtService<UserClaims> jwtService)
        {
            _jwtService = jwtService;
        }

        public async ValueTask<Result<CommandResponse, Exception>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var result = await _jwtService.RefreshAccessToken(request.RefreshToken, cancellationToken);
            return result.Match<Result<CommandResponse, Exception>>(
                accessToken => new CommandResponse
                {
                    AccessToken = accessToken
                },
                exception => exception);
        }
    }
}