namespace MusicStreamingService.Data.Entities;

public class PlaylistFavoriteEntity
{
    /// <summary>
    /// Id of the user, who liked this playlist
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User, who liked this playlist
    /// </summary>
    public UserEntity User { get; set; } = null!;
    
    /// <summary>
    /// Id of the liked playlist
    /// </summary>
    public Guid PlaylistId { get; set; }

    /// <summary>
    /// Liked playlist
    /// </summary>
    public PlaylistEntity Playlist { get; set; } = null!;
}