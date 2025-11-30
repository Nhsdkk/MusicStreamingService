namespace MusicStreamingService.Data.Entities;

public class SongGenreEntity
{
    /// <summary>
    /// Id of the song
    /// </summary>
    public Guid SongId { get; set; }

    /// <summary>
    /// Song
    /// </summary>
    public SongEntity Song { get; set; } = null!;

    /// <summary>
    /// Id of the genre, to which song will correspond
    /// </summary>
    public Guid GenreId { get; set; }

    /// <summary>
    /// Genre, to which song will correspond
    /// </summary>
    public GenreEntity Genre { get; set; } = null!;
}