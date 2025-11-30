namespace MusicStreamingService.Data.Entities;

public sealed record SongArtistEntity
{
    /// <summary>
    /// Id of the song 
    /// </summary>
    public Guid SongId { get; set; }

    /// <summary>
    /// Song
    /// </summary>
    public SongEntity Song { get; set; } = null!;
    
    /// <summary>
    /// Id of the artist, who contributed to the song production
    /// </summary>
    public Guid ArtistId { get; set; }

    /// <summary>
    /// Artist, who contributed to the song production
    /// </summary>
    public UserEntity Artist { get; set; } = null!;
    
    /// <summary>
    /// Flag, that determines if artist is main artist of the song
    /// </summary>
    public bool MainArtist { get; set; }
}