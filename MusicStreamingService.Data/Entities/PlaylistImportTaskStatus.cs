namespace MusicStreamingService.Data.Entities;

public enum PlaylistImportTaskStatus
{
    /// <summary>
    /// Task has been created but not yet started
    /// </summary>
    Created,
    
    /// <summary>
    /// Task is currently in progress
    /// </summary>
    Processing,
    
    /// <summary>
    /// Task finished
    /// </summary>
    Finished,
}