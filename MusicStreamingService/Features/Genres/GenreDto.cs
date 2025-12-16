using System.Text.Json.Serialization;
using MusicStreamingService.Data.Entities;

namespace MusicStreamingService.Features.Genres;

public sealed record GenreDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = null!;

    public static GenreDto FromEntity(GenreEntity genre) =>
        new GenreDto
        {
            Id = genre.Id,
            Title = genre.Title
        };
}