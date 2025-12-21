using MusicStreamingService.Data.Interceptors;

namespace MusicStreamingService.Data.Entities;

public sealed record PermissionEntity : BaseUpdatableIdEntity, IAuditable
{
    /// <summary>
    /// Name of the permission
    /// </summary>
    public string Title { get; set; } = null!;
    
    /// <summary>
    /// Description of the permission
    /// </summary>
    public string Description { get; set; } = null!;
}
