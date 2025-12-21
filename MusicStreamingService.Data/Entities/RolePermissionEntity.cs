using MusicStreamingService.Data.Interceptors;

namespace MusicStreamingService.Data.Entities;

public sealed class RolePermissionEntity : IAuditable
{
    public Guid RoleId { get; set; }
    
    public RoleEntity Role { get; set; } = null!;
    
    public Guid PermissionId { get; set; }
    
    public PermissionEntity Permission { get; set; } = null!;
}
