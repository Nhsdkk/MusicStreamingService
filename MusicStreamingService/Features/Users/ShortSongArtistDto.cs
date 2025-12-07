using System.Text.Json.Serialization;
using MusicStreamingService.Data.Entities;

namespace MusicStreamingService.Features.Users;

public sealed record ShortSongArtistDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("username")]
    public string Username { get; init; } = null!;

    [JsonPropertyName("mainArtist")]
    public bool MainArtist { get; init; }

    public static ShortSongArtistDto FromEntity(UserEntity artist, bool mainArtist) =>
        new ShortSongArtistDto()
        {
            Id = artist.Id,
            Username = artist.Username,
            MainArtist = mainArtist
        };
}