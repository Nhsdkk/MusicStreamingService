using MusicStreamingService.Data.Interceptors;

namespace MusicStreamingService.Data.Entities;

public sealed record RegionEntity : BaseIdEntity, IAuditable
{
    /// <summary>
    /// Name of the region
    /// </summary>
    public string Title { get; set; } = null!;
}
