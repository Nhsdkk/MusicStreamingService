using MusicStreamingService.Data.Interceptors;

namespace MusicStreamingService.Data.Entities;

public class UserRoleEntity : IAuditable
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
    /// Id of user's role
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// User's role
    /// </summary>
    public RoleEntity Role { get; set; } = null!;
}
