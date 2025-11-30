namespace MusicStreamingService.Data.Entities;

public sealed record AllowedDistributionEntity
{
    /// <summary>
    /// Id of the song
    /// </summary>
    public Guid SongId { get; set; }

    /// <summary>
    /// Song entity
    /// </summary>
    public SongEntity Song { get; set; } = null!;
    
    /// <summary>
    /// Id of the region, where song is allowed for distribution
    /// </summary>
    public Guid RegionId { get; set; }

    /// <summary>
    /// Region, where song is allowed for distribution
    /// </summary>
    public RegionEntity Region { get; set; } = null!;
}