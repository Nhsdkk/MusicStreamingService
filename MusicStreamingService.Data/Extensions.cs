using MusicStreamingService.Data.Entities;

namespace MusicStreamingService.Data;

public static class Extensions
{
    public static List<PermissionEntity> GetPermissions(this UserEntity user) =>
        user.Roles.SelectMany(x => x.Permissions).ToList();
}