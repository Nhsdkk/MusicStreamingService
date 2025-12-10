using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.DateUtils;

namespace MusicStreamingService.Extensions;

public static class ClaimsExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal) =>
        Guid.Parse(claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sid)!);

    public static RegionClaim GetUserRegion(this ClaimsPrincipal claimsPrincipal) =>
        System.Text.Json.JsonSerializer.Deserialize<RegionClaim>(
            claimsPrincipal.FindFirstValue(CustomClaimTypes.RegionsClaimType)!)!;

    public static int GetUserAge(this ClaimsPrincipal claimsPrincipal)
    {
        var birthDate = DateTime.ParseExact(
            claimsPrincipal.FindFirstValue(CustomClaimTypes.BirthDateClaimType)!,
            DateFormats.FullDateFormat,
            null).ToUniversalTime();
        
        return new DateTime((DateTime.UtcNow - birthDate).Ticks).Year - 1;
    }
}