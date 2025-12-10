using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using MusicStreamingService.Infrastructure.DateUtils;
using MusicStreamingService.Infrastructure.Result;

namespace MusicStreamingService.Infrastructure.Authentication;

/// <summary>
/// Interface to convert claims to user data and back
/// </summary>
/// <typeparam name="T">Type of user data</typeparam>
public interface IClaimConverter<T>
{
    /// <summary>
    /// Create user data from claims
    /// </summary>
    /// <param name="claims">Claims</param>
    /// <returns></returns>
    public Result<T, Exception> FromClaims(List<Claim> claims);

    /// <summary>
    /// Convert user data to claims
    /// </summary>
    /// <param name="data">User's data</param>
    /// <returns></returns>
    public IEnumerable<Claim> ToClaims(T data);
}

public sealed class ClaimConverter : IClaimConverter<UserClaims>
{
    public Result<UserClaims, Exception> FromClaims(List<Claim> claims)
    {
        var permissions = claims.Where(x => x.Type is ClaimTypes.Role).Select(x => x.Value).ToList();

        var username = claims.FirstOrDefault(x => x.Type is JwtRegisteredClaimNames.Name)?.Value;
        if (username is null)
        {
            return new JwtValidationException("Can't get username claim");
        }

        var idClaim = claims.FirstOrDefault(x => x.Type is JwtRegisteredClaimNames.Sid);
        if (idClaim is null)
        {
            return new JwtValidationException("Can't get user id claim");
        }

        if (!Guid.TryParse(idClaim.Value, out var id))
        {
            return new JwtValidationException("Can't parse user id claim");
        }

        var regionClaim = claims.FirstOrDefault(x => x.Type is CustomClaimTypes.RegionsClaimType);
        if (regionClaim is null)
        {
            return new JwtValidationException("Can't get region claim");
        }

        var birthDateClaim = claims.FirstOrDefault(x => x.Type is CustomClaimTypes.BirthDateClaimType);
        if (birthDateClaim is null)
        {
            return new JwtValidationException("Can't get birth date claim");
        }
        
        var birthDate = DateTime.ParseExact(birthDateClaim.Value, DateFormats.FullDateFormat, null);

        try
        {
            var region = JsonSerializer.Deserialize<RegionClaim>(regionClaim.Value);
            if (region is null || region.Id == Guid.Empty)
            {
                return new JwtValidationException("Can't parse region id claim");
            }

            return new UserClaims
            {
                Permissions = permissions,
                Id = id,
                Region = region,
                Username = username,
                BirthDate = birthDate
            };
        }
        catch (JsonException)
        {
            return new JwtValidationException("Can't parse region claim");
        }
    }

    public IEnumerable<Claim> ToClaims(UserClaims data)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Name, data.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sid, data.Id.ToString()),
            new Claim(CustomClaimTypes.RegionsClaimType, JsonSerializer.Serialize(data.Region)),
            new Claim(CustomClaimTypes.BirthDateClaimType, data.BirthDate.ToString(DateFormats.FullDateFormat))
        };

        claims.AddRange(data.Permissions.Select(x => new Claim(ClaimTypes.Role, x)));
        return claims;
    }
}