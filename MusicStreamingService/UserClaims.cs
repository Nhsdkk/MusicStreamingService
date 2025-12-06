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
}