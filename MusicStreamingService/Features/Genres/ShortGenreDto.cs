using System.Text.Json.Serialization;
using MusicStreamingService.Data.Entities;

namespace MusicStreamingService.Features.Genres;

public sealed record ShortGenreDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = null!;

    public static ShortGenreDto FromEntity(GenreEntity genre) =>
        new ShortGenreDto
        {
            Id = genre.Id,
            Title = genre.Title
        };
}