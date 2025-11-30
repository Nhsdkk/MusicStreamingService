namespace MusicStreamingService.Data.Entities;

public sealed record StreamingEventEntity : BaseIdEntity
{
    /// <summary>
    /// Id of the song playing
    /// </summary>
    public Guid SongId { get; set; }

    /// <summary>
    /// Song playing
    /// </summary>
    public SongEntity Song { get; set; } = null!;
    
    /// <summary>
    /// Id of the device, which is playing the song
    /// </summary>
    public Guid DeviceId { get; set; }

    /// <summary>
    /// Device, which is playing the song
    /// </summary>
    public DeviceEntity Device { get; set; } = null!;
    
    /// <summary>
    /// Requested track position in milliseconds 
    /// </summary>
    public long PositionMs { get; set; }
    
    /// <summary>
    /// Time played since last event submitted in milliseconds
    /// </summary>
    public long TimePlayedSinceLastRequestMs { get; set; }
    
    /// <summary>
    /// Type of the event <see cref="StreamingEventType"/>
    /// </summary>
    public StreamingEventType EventType { get; set; }
}