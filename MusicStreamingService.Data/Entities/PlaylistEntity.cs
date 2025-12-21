using MusicStreamingService.Data.Interceptors;

namespace MusicStreamingService.Data.Entities;

public sealed record PlaylistEntity : BaseUpdatableIdEntity, IAuditable
{
    /// <summary>
    /// Name of the playlist
    /// </summary>
    public string Title { get; set; } = null!;
    
    /// <summary>
    /// Description of the playlist
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Id of playlist creator
    /// </summary>
    public Guid CreatorId { get; set; }

    /// <summary>
    /// Playlist creator
    /// </summary>
    public UserEntity Creator { get; set; } = null!;
    
    /// <summary>
    /// Playlist access type (either <see cref="PlaylistAccessType.Public"/> or <see cref="PlaylistAccessType.Private"/>)
    /// </summary>
    public PlaylistAccessType AccessType { get; set; }

    /// <summary>
    /// Amount of likes on the playlist
    /// </summary>
    public long Likes { get; } = 0;

    /// <summary>
    /// Playlist's songs
    /// </summary>
    public List<PlaylistSongEntity> Songs { get; set; } = new List<PlaylistSongEntity>();
    
    /// <summary>
    /// Users who liked the playlist
    /// </summary>
    public List<UserEntity> LikedByUsers { get; set; } = new List<UserEntity>();
}
