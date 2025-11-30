namespace MusicStreamingService.Data.Entities;

public sealed record PermissionEntity : BaseUpdatableIdEntity
{
    /// <summary>
    /// Name of the permission
    /// </summary>
    public string Title { get; set; } = null!;
    
    /// <summary>
    /// Description of the permission
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// User's using current permission
    /// </summary>
    public List<UserEntity> UsedBy { get; set; } = new List<UserEntity>();
}