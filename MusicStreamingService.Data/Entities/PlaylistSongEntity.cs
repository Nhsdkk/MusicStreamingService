namespace MusicStreamingService.Data.Entities;

public sealed record PlaylistSongEntity
{
    /// <summary>
    /// Id of the playlist, to which song is assigned
    /// </summary>
    public Guid PlaylistId { get; set; }

    /// <summary>
    /// Playlist, to which song is assigned
    /// </summary>
    public PlaylistEntity Playlist { get; set; } = null!;
    
    /// <summary>
    /// Id of the song, assigned to the playlist
    /// </summary>
    public Guid SongId { get; set; }

    /// <summary>
    /// Song, assigned to the playlist
    /// </summary>
    public SongEntity Song { get; set; } = null!;
    
    /// <summary>
    /// Position of the track in the playlist
    /// </summary>
    public long Position { get; set; }
}