using MusicStreamingService.Data.Interceptors;

namespace MusicStreamingService.Data.Entities;

public sealed record AuditLogEntity : BaseIdEntity
{
    /// <summary>
    /// Action that was performed
    /// </summary>
    public EntityAction Action { get; init; }

    /// <summary>
    /// New data
    /// </summary>
    public string NewValues { get; init; } = null!;
    
    /// <summary>
    /// Old data
    /// </summary>
    public string OldValues { get; init; } = null!;
    
    /// <summary>
    /// Table name of the entity that was changed
    /// </summary>
    public string TableName { get; init; } = null!;
}
