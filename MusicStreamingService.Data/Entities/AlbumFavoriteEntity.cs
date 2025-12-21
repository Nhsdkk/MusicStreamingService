using MusicStreamingService.Data.Interceptors;

namespace MusicStreamingService.Data.Entities;

public sealed record AlbumFavoriteEntity : IAuditable
{
    /// <summary>
    /// Id of the user, who liked this album
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User, who liked the album
    /// </summary>
    public UserEntity User { get; set; } = null!;
    
    /// <summary>
    /// Id of the corresponding liked album
    /// </summary>
    public Guid AlbumId { get; set; }
    
    /// <summary>
    /// Liked album
    /// </summary>
    public AlbumEntity Album { get; set; } = null!;
}
