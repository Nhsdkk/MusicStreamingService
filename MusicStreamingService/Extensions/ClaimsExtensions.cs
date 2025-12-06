using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MusicStreamingService.Infrastructure.Authentication;

namespace MusicStreamingService.Extensions;

public static class ClaimsExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal) =>
        Guid.Parse(claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sid)!);

    public static RegionClaim GetUserRegion(this ClaimsPrincipal claimsPrincipal) =>
        System.Text.Json.JsonSerializer.Deserialize<RegionClaim>(
            claimsPrincipal.FindFirstValue(CustomClaimTypes.RegionsClaimType)!)!;
}