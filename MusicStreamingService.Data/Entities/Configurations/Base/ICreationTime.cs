namespace MusicStreamingService.Data.Entities.Configurations.Base;

public interface ICreationTime
{
    /// <summary>
    /// Entity creation time
    /// </summary>
    public DateTime CreatedAt { get; set; }
}