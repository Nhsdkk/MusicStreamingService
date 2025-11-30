namespace MusicStreamingService.Data.Entities;

public sealed record RegionEntity : BaseIdEntity
{
    /// <summary>
    /// Name of the region
    /// </summary>
    public string Title { get; set; } = null!;
}