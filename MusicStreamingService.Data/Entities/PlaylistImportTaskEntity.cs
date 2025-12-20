namespace MusicStreamingService.Data.Entities;

public sealed record PlaylistImportTaskEntity : BaseUpdatableIdEntity
{
    /// <summary>
    /// Id of the user who created the import task
    /// </summary>
    public Guid CreatorId { get; set; }

    /// <summary>
    /// User who created the import task
    /// </summary>
    public UserEntity Creator { get; set; } = null!;
    
    /// <summary>
    /// Task status <see cref="PlaylistImportTaskStatus"/>
    /// </summary>
    public PlaylistImportTaskStatus Status { get; set; }
    
    /// <summary>
    /// Filename of the json file to process inside S3 storage
    /// </summary>
    public string S3FileName { get; set; } = null!;
    
    /// <summary>
    /// Entries staged for processing
    /// </summary>
    public List<PlaylistImportStagingEntity> StagingEntries { get; set; } = new();
    
    /// <summary>
    /// Total entries to process
    /// </summary>
    public long TotalEntries { get; set; }

    /// <summary>
    /// Entries already processed
    /// </summary>
    public long ProcessedEntries { get; } = 0;
}