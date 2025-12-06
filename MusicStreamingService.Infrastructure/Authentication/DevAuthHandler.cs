using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
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
            new Claim(ClaimTypes.Role, Permissions.ViewSongsPermission),
            new Claim(ClaimTypes.Role, Permissions.ManageSongsPermission),
            new Claim(ClaimTypes.Role, Permissions.PlaySongsPermission),
            new Claim(ClaimTypes.Role, Permissions.FavoriteSongsPermission),
            new Claim(ClaimTypes.Role, Permissions.AdministrateSongsPermission),

            new Claim(ClaimTypes.Role, Permissions.ViewAlbumsPermission),
            new Claim(ClaimTypes.Role, Permissions.FavoriteAlbumsPermission),
            new Claim(ClaimTypes.Role, Permissions.ManageAlbumsPermission),
            new Claim(ClaimTypes.Role, Permissions.AdministrateAlbumsPermission),

            new Claim(ClaimTypes.Role, Permissions.ViewUsersPermission),
            new Claim(ClaimTypes.Role, Permissions.ManageUsersPermission),
            new Claim(ClaimTypes.Role, Permissions.AdministrateUsersPermission),

            new Claim(ClaimTypes.Role, Permissions.ViewSubscriptionsPermission),
            new Claim(ClaimTypes.Role, Permissions.ManageSubscriptionsPermission),
            new Claim(ClaimTypes.Role, Permissions.AdministrateSubscriptionsPermission),

            new Claim(ClaimTypes.Role, Permissions.ViewPlaylistsPermission),
            new Claim(ClaimTypes.Role, Permissions.FavoritePlaylistsPermission),
            new Claim(ClaimTypes.Role, Permissions.ManagePlaylistsPermission),
            new Claim(ClaimTypes.Role, Permissions.AdministratePlaylistsPermission),

            new Claim(ClaimTypes.Role, Permissions.ViewGenresPermission),
            new Claim(ClaimTypes.Role, Permissions.AdministrateGenresPermission),

            new Claim(ClaimTypes.Role, Permissions.ViewRegionsPermission),
            new Claim(ClaimTypes.Role, Permissions.AdministrateRegionsPermission),

            new Claim(ClaimTypes.Role, Permissions.ViewStreamingEventsPermission),
            new Claim(ClaimTypes.Role, Permissions.ManageStreamingEventsPermission),
            new Claim(ClaimTypes.Role, Permissions.AdministrateStreamingEventsPermission),

            new Claim(ClaimTypes.Role, Permissions.ViewPaymentsPermission),
            new Claim(ClaimTypes.Role, Permissions.ManagePaymentsPermission),
            new Claim(ClaimTypes.Role, Permissions.AdministratePaymentsPermission),
            
            new Claim(CustomClaimTypes.RegionsClaimType, JsonSerializer.Serialize(new RegionClaim
            {
                Id = Guid.Parse("ee76a7aa-7ca4-4df9-9ab2-eb5d8d8d58ac"),
                Title = "Russia"
            })),
            
        ], authenticationType: Scheme.Name);

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(adminClaims), Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}