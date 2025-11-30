namespace MusicStreamingService.Data.Entities;

public sealed record SongFavoriteEntity
{
    /// <summary>
    /// Id of the song, which user likes
    /// </summary>
    public Guid SongId { get; set; }

    /// <summary>
    /// Liked song
    /// </summary>
    public SongEntity Song { get; set; } = null!;
    
    /// <summary>
    /// Id of the user, who liked the song
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User, who liked the song
    /// </summary>
    public UserEntity User { get; set; } = null!;
}