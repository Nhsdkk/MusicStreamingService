using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Infrastructure.Authentication;

namespace MusicStreamingService;

public class UserClaims : IClaimConvertable
{
    private readonly IEnumerable<string> _permissions;
    private readonly string _username;
    private readonly Guid _id;
    private readonly RegionClaim _region;
    private readonly DateTime _birthDate;

    public UserClaims(UserEntity user)
    {
        _id = user.Id;
        _permissions = user.GetPermissions().Select(x => x.Title);
        _username = user.Username;
        _region = new RegionClaim
        {
            Id = user.Region.Id,
            Title = user.Region.Title
        };
        _birthDate = user.BirthDate;
    }

    public IEnumerable<string> GetPermissions() => _permissions;

    public string GetUsername() => _username;

    public Guid GetId() => _id;

    public RegionClaim GetRegion() => _region;

    public DateTime GetBirthDate() => _birthDate;

    public List<Claim> ToClaims()
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Name, _username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sid, _id.ToString()),
            new Claim(CustomClaimTypes.RegionsClaimType, System.Text.Json.JsonSerializer.Serialize(_region)),
            new Claim(CustomClaimTypes.BirthDateClaimType, _birthDate.ToString("MM/dd/yyyy"))
        };

        claims.AddRange(_permissions.Select(x => new Claim(ClaimTypes.Role, x)));
        return claims;
    }
}