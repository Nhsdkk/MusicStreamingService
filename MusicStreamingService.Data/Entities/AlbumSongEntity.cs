namespace MusicStreamingService.Data.Entities;

public sealed record AlbumSongEntity
{
    /// <summary>
    /// Id of the album, to which song is related
    /// </summary>
    public Guid AlbumId { get; set; }

    /// <summary>
    /// Album, to which song is related
    /// </summary>
    public AlbumEntity Album { get; set; } = null!;
    
    /// <summary>
    /// Id of the song in the album
    /// </summary>
    public Guid SongId { get; set; }

    /// <summary>
    /// Song, that is inside the album
    /// </summary>
    public SongEntity Song { get; set; } = null!;
    
    /// <summary>
    /// Flag, that determines if song is title song or not
    /// </summary>
    public bool Title { get; set; }
    
    /// <summary>
    /// Position of the song inside the album
    /// </summary>
    public long Position { get; set; }
}