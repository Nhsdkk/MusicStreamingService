namespace MusicStreamingService.Data.Entities;

public sealed record SongEntity : BaseUpdatableIdEntity
{
    /// <summary>
    /// Song title
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Song duration in milliseconds
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Song filename inside s3 object storage 
    /// </summary>
    public string S3MediaFileName { get; set; } = null!;

    /// <summary>
    /// Amount of song likes
    /// </summary>
    public long Likes { get; } = 0;
    
    /// <summary>
    /// Flag for explicit songs
    /// </summary>
    public bool Explicit { get; set; }

    /// <summary>
    /// Artists, who contributed to the creation of the song
    /// </summary>
    public List<SongArtistEntity> Artists { get; set; } = new List<SongArtistEntity>();

    /// <summary>
    /// Regions, where song is available for distribution 
    /// </summary>
    public List<RegionEntity> AllowedRegions { get; set; } = new List<RegionEntity>();

    /// <summary>
    /// Album in which song appears 
    /// </summary>
    public AlbumEntity Album { get; set; } = null!;
    
    /// <summary>
    /// Id of the album in which song appears
    /// </summary>
    public Guid AlbumId { get; set; }
    
    /// <summary>
    /// Flag to determine if the song is the title track of the album
    /// </summary>
    public bool IsTitleTrack { get; set; }
    
    /// <summary>
    /// Position of the song within the album
    /// </summary>
    public long AlbumPosition { get; set; }

    /// <summary>
    /// Song genres
    /// </summary>
    public List<GenreEntity> Genres { get; set; } = new List<GenreEntity>();
    
    /// <summary>
    /// Users who liked the song
    /// </summary>
    public List<UserEntity> LikedByUsers { get; set; } = new List<UserEntity>();
}