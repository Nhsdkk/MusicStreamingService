using MusicStreamingService.Data.Interceptors;

namespace MusicStreamingService.Data.Entities;

public sealed record DeviceEntity : BaseIdEntity, IAuditable
{
    /// <summary>
    /// Name of the device
    /// </summary>
    public string Title { get; set; } = null!;
    
    /// <summary>
    /// Id of the owner of the device
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Owner of the device
    /// </summary>
    public UserEntity Owner { get; set; } = null!;

    /// <summary>
    /// Streaming events, corresponding to this device
    /// </summary>
    public List<StreamingEventEntity> StreamingEvents { get; set; } = new List<StreamingEventEntity>();
}
