namespace MusicStreamingService.Data.Entities;

public sealed record RoleEntity : BaseUpdatableIdEntity
{
    /// <summary>
    /// Title of the role
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Description of the role
    /// </summary>
    public string Description { get; set; } = null!;
    
    /// <summary>
    /// Permissions related to this role
    /// </summary>
    public List<PermissionEntity> Permissions { get; set; } = new List<PermissionEntity>();
}