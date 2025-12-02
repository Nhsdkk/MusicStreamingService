using System.Security.Claims;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Infrastructure.Authentication;

namespace MusicStreamingService;

public class UserClaims : IClaimConvertable
{
    private readonly IEnumerable<string> _permissions;
    private readonly string _username;
    
    public UserClaims(UserEntity user)
    {
        _permissions = user.Roles.Aggregate(new List<string>(), (list, role) => [..list, role.Title]);
        _username = user.Username;
    }

    public IEnumerable<string> GetPermissions() => _permissions;

    public string GetUsername() => _username;
}