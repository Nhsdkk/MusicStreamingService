using MusicStreamingService.Data.Interceptors;

namespace MusicStreamingService.Data.Entities;

public sealed record GenreEntity : BaseUpdatableIdEntity, IAuditable
{
    /// <summary>
    /// Name of the genre
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Description of the genre
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Songs of this genre
    /// </summary>
    public List<SongEntity> Songs { get; set; } = new List<SongEntity>();
}
