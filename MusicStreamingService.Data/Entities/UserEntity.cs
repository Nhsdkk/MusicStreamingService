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
    /// User's birth date
    /// </summary>
    public DateTime BirthDate { get; set; }
    
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

    /// <summary>
    /// User's favorite albums
    /// </summary>
    public List<AlbumEntity> FavoriteAlbums { get; set; } = new List<AlbumEntity>();

    /// <summary>
    /// User's albums
    /// </summary>
    public List<AlbumEntity> ArtistAlbums { get; set; } = new List<AlbumEntity>();

    /// <summary>
    /// User's songs
    /// </summary>
    public List<SongArtistEntity> ArtistSongs { get; set; } = new List<SongArtistEntity>();
    
    /// <summary>
    /// User's favorite songs
    /// </summary>
    public List<SongEntity> FavoriteSongs { get; set; } = new List<SongEntity>();

    /// <summary>
    /// User's subscriptions
    /// </summary>
    public List<SubscriberEntity> Subscriptions { get; set; } = new List<SubscriberEntity>();

    /// <summary>
    /// User's payments
    /// </summary>
    public List<PaymentEntity> Payments { get; set; } = new List<PaymentEntity>();

    /// <summary>
    /// Playlists created by user
    /// </summary>
    public List<PlaylistEntity> OwnedPlaylists { get; set; } = new List<PlaylistEntity>();

    /// <summary>
    /// Favorite user's playlists
    /// </summary>
    public List<PlaylistEntity> FavoritePlaylists { get; set; } = new List<PlaylistEntity>();

    /// <summary>
    /// User's devices
    /// </summary>
    public List<DeviceEntity> Devices { get; set; } = new List<DeviceEntity>();
};