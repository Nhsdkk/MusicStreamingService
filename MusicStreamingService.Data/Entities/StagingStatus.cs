namespace MusicStreamingService.Data.Entities;

public enum StagingStatus
{
    /// <summary>
    /// Entry awaits matching
    /// </summary>
    Pending,
    
    /// <summary>
    /// Entry matched to the song successfully
    /// </summary>
    Processed,
    
    /// <summary>
    /// Song matching failed
    /// </summary>
    Failed
}

