using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MusicStreamingService.Infrastructure.Authentication;

public class DevAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DevAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var adminClaims = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Role, "mss.tracks.view"),
            new Claim(ClaimTypes.Role, "mss.tracks.manage"),
            new Claim(ClaimTypes.Role, "mss.tracks.playback"),
            new Claim(ClaimTypes.Role, "mss.tracks.favorite"),
            new Claim(ClaimTypes.Role, "mss.tracks.admin"),

            new Claim(ClaimTypes.Role, "mss.albums.view"),
            new Claim(ClaimTypes.Role, "mss.albums.favorite"),
            new Claim(ClaimTypes.Role, "mss.albums.manage"),
            new Claim(ClaimTypes.Role, "mss.albums.admin"),

            new Claim(ClaimTypes.Role, "mss.users.view"),
            new Claim(ClaimTypes.Role, "mss.users.manage"),
            new Claim(ClaimTypes.Role, "mss.users.admin"),

            new Claim(ClaimTypes.Role, "mss.subscriptions.view"),
            new Claim(ClaimTypes.Role, "mss.subscriptions.manage"),
            new Claim(ClaimTypes.Role, "mss.subscriptions.admin"),

            new Claim(ClaimTypes.Role, "mss.playlists.view"),
            new Claim(ClaimTypes.Role, "mss.playlists.favorite"),
            new Claim(ClaimTypes.Role, "mss.playlists.manage"),
            new Claim(ClaimTypes.Role, "mss.playlists.admin"),

            new Claim(ClaimTypes.Role, "mss.genres.view"),
            new Claim(ClaimTypes.Role, "mss.genres.admin"),

            new Claim(ClaimTypes.Role, "mss.regions.view"),
            new Claim(ClaimTypes.Role, "mss.regions.admin"),

            new Claim(ClaimTypes.Role, "mss.streaming-events.view"),
            new Claim(ClaimTypes.Role, "mss.streaming-events.manage"),
            new Claim(ClaimTypes.Role, "mss.streaming-events.admin"),

            new Claim(ClaimTypes.Role, "mss.payments.view"),
            new Claim(ClaimTypes.Role, "mss.payments.manage"),
            new Claim(ClaimTypes.Role, "mss.payments.admin")
        ], authenticationType: Scheme.Name);

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(adminClaims), Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}