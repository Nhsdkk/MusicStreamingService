namespace MusicStreamingService.Data.Entities;

public sealed record PlaylistImportStagingEntity : BaseUpdatableIdEntity
{
    /// <summary>
    /// Id of the batch this entry belongs to
    /// </summary>
    public Guid BatchId { get; set; }
    
    /// <summary>
    /// Id of the playlist import task this entry belongs to
    /// </summary>
    public Guid ImportTaskId { get; set; }
    
    /// <summary>
    /// Title of the song to be imported
    /// </summary>
    public string SongTitle { get; set; } = null!;

    /// <summary>
    /// Album name of the song to be imported
    /// </summary>
    public string AlbumName { get; set; } = null!;
    
    /// <summary>
    /// Name of any writer associated with the song
    /// </summary>
    public string ArtistName { get; set; } = null!;
    
    /// <summary>
    /// Song release date
    /// </summary>
    public DateOnly ReleaseDate { get; set; }
    
    /// <summary>
    /// Staging entry status <see cref="StagingStatus"/>
    /// </summary>
    public StagingStatus Status { get; set; }
    
    /// <summary>
    /// Id of the playlist to which the song will be added
    /// </summary>
    public Guid PlaylistId { get; set; }

    /// <summary>
    /// Playlist to which the song will be added
    /// </summary>
    public PlaylistEntity Playlist { get; set; } = null!;
}