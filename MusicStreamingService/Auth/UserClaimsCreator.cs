using MusicStreamingService.Data.Entities;
using MusicStreamingService.Infrastructure.Authentication;

namespace MusicStreamingService.Auth;

public static class UserClaimsCreator
{
    public static UserClaims FromEntity(UserEntity user) =>
        new UserClaims()
        {
            Id = user.Id,
            Username = user.Username,
            Permissions = user.Roles.SelectMany(x => x.Permissions).Select(p => p.Title).ToList(),
            Region = new RegionClaim()
            {
                Id = user.Region.Id,
                Title = user.Region.Title
            },
            BirthDate = user.BirthDate
        };
}