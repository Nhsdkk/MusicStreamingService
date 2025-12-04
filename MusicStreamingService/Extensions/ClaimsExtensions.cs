using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MusicStreamingService.Extensions;

public static class ClaimsExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal) =>
        Guid.Parse(claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sid)!);
}