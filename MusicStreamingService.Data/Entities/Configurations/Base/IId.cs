namespace MusicStreamingService.Data.Entities.Configurations.Base;

public interface IId<TKey>
{
    /// <summary>
    /// Entity id
    /// </summary>
    public TKey Id { get; set; }
}