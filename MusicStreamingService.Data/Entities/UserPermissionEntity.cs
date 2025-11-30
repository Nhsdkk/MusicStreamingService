namespace MusicStreamingService.Data.Entities;

public class UserPermissionEntity
{
    /// <summary>
    /// Id of the user, who has this permission
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User, who has this permission
    /// </summary>
    public UserEntity User { get; set; } = null!;

    /// <summary>
    /// Id of user's permission
    /// </summary>
    public Guid PermissionId { get; set; }

    /// <summary>
    /// User's permission
    /// </summary>
    public PermissionEntity Permission { get; set; } = null!;
}