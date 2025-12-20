using System.Security.Claims;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.DateUtils;

namespace MusicStreamingService.Extensions;

public static class ClaimsExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal) =>
        Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.Sid)!);

    public static RegionClaim GetUserRegion(this ClaimsPrincipal claimsPrincipal) =>
        System.Text.Json.JsonSerializer.Deserialize<RegionClaim>(
            claimsPrincipal.FindFirstValue(CustomClaimTypes.RegionsClaimType)!)!;

    public static int GetUserAge(this ClaimsPrincipal claimsPrincipal)
    {
        var birthDate = DateTime.ParseExact(
            claimsPrincipal.FindFirstValue(ClaimTypes.DateOfBirth)!,
            DateFormats.FullDateFormat,
            null).ToUniversalTime();

        var age = DateTime.UtcNow.Year - birthDate.Year;
        if (birthDate.AddYears(age) > DateTime.UtcNow)
        {
            age--;
        }
        return age;
    }
}