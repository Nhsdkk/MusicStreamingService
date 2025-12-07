using System.Text.Json.Serialization;
using MusicStreamingService.Data.Entities;

namespace MusicStreamingService.Features.Albums;

public sealed record ShortAlbumDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = null!;

    [JsonPropertyName("likes")]
    public long Likes { get; init; }

    [JsonPropertyName("albumArtUrl")]
    public string AlbumArtUrl { get; init; } = null!;

    public static ShortAlbumDto FromEntity(
        AlbumEntity album,
        string albumArtUrl) =>
        new ShortAlbumDto
        {
            Id = album.Id,
            Title = album.Title,
            Likes = album.Likes,
            AlbumArtUrl = albumArtUrl
        };
}