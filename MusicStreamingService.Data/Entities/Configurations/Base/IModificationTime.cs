namespace MusicStreamingService.Data.Entities.Configurations.Base;

public interface IModificationTime
{
    /// <summary>
    /// Entity last modification time
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}