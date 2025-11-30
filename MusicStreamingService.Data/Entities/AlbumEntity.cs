namespace MusicStreamingService.Data.Entities;

public sealed record AlbumEntity : BaseUpdatableIdEntity
{
    /// <summary>
    /// Album title
    /// </summary>
    public string Title { get; set; } = null!;
    
    /// <summary>
    /// Album description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Like count
    /// </summary>
    public int Likes { get; } = 0;
    
    /// <summary>
    /// Id of the corresponding creator
    /// </summary>
    public Guid ArtistId { get; set; }

    /// <summary>
    /// Creator of the album
    /// </summary>
    public UserEntity Artist { get; set; } = null!;
    
    /// <summary>
    /// Release date of the album
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// User's who liked this album
    /// </summary>
    public List<UserEntity> LikedUsers { get; set; } = new List<UserEntity>();

    /// <summary>
    /// Filename of the album artwork inside s3 object storage
    /// </summary>
    public string S3ArtworkFilename { get; set; } = null!;
}