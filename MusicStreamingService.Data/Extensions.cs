using MusicStreamingService.Data.Entities;

namespace MusicStreamingService.Data;

public static class Extensions
{
    public static List<PermissionEntity> GetPermissions(this UserEntity user) =>
        user.Roles.Aggregate(new List<PermissionEntity>(), (list, r) => [..list, ..r.Permissions]);
}