namespace MusicStreamingService.Data.Entities;

public sealed record UserEntity : BaseUpdatableIdEntity
{
    /// <summary>
    /// User's email
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// User's full name
    /// </summary>
    public string FullName { get; set; } = null!;

    /// <summary>
    /// User's username
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// User's password
    /// </summary>
    public byte[] Password { get; set; } = null!;

    /// <summary>
    /// User's region
    /// </summary>
    public RegionEntity Region { get; set; } = null!;
    
    /// <summary>
    /// Id of the user's region
    /// </summary>
    public Guid RegionId { get; set; }

    /// <summary>
    /// Flag, that checks if user's account is disabled
    /// </summary>
    public bool Disabled { get; set; } = false;
};