using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.Result;

namespace MusicStreamingService.Auth;

public sealed class UserClaims : IClaimConvertable
{
    private IEnumerable<string> _permissions;
    private string _username;
    private Guid _id;
    private RegionClaim _region;

    public UserClaims()
    {
        _permissions = new List<string>();
        _username = string.Empty;
        _id = Guid.Empty;
        _region = new RegionClaim();
    }
    
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
    }

    public IEnumerable<string> GetPermissions() => _permissions;

    public string GetUsername() => _username;

    public Guid GetId() => _id;

    public RegionClaim GetRegion() => _region;
    
    public Result<IClaimConvertable, Exception> FromClaims(List<Claim> claims)
    {
        _permissions = claims.Where(x => x.Type is ClaimTypes.Role).Select(x => x.Value);
        
        var username = claims.FirstOrDefault(x => x.Type is JwtRegisteredClaimNames.Name)?.Value;
        if (username is null)
        {
            return new JwtValidationException("Can't get username claim");   
        }
        _username = username;

        var idClaim = claims.FirstOrDefault(x => x.Type is JwtRegisteredClaimNames.Sid);
        if (idClaim is null)
        {
            return new JwtValidationException("Can't get user id claim");
        }
        
        if (!Guid.TryParse(idClaim.Value, out var id))
        {
            return new JwtValidationException("Can't parse user id claim");
        }
        _id = id;
        
        var regionClaim = claims.FirstOrDefault(x => x.Type is CustomClaimTypes.RegionsClaimType);
        if (regionClaim is null)
        {
            return new JwtValidationException("Can't get region claim");
        }

        try
        {
            var region = JsonSerializer.Deserialize<RegionClaim>(regionClaim.Value);
            if (region is null || region.Id == Guid.Empty)
            {
                return new JwtValidationException("Can't parse region id claim");
            }
            _region = region;
            
            return this;
        }
        catch (JsonException)
        {
            return new JwtValidationException("Can't parse region claim");
        }
    }
}