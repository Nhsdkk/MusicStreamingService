using System.Text.Json.Serialization;
using MusicStreamingService.Data.Entities;

namespace MusicStreamingService.Features.Albums;

public sealed record ShortAlbumDto
{
    // TODO: add album art url
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = null!;

    [JsonPropertyName("likes")]
    public long Likes { get; init; }

    public static ShortAlbumDto FromEntity(
        AlbumEntity album) =>
        new ShortAlbumDto
        {
            Id = album.Id,
            Title = album.Title,
            Likes = album.Likes
        };
}