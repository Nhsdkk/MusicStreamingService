using System.Text.Json.Serialization;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Features.Genres;
using MusicStreamingService.Features.Region;
using MusicStreamingService.Features.Users;

namespace MusicStreamingService.Features.Songs;

public sealed record ShortSongDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = null!;

    [JsonPropertyName("artists")]
    public List<ShortSongArtistDto> Artists { get; init; } = null!;

    [JsonPropertyName("durationMs")]
    public long DurationMs { get; init; }

    [JsonPropertyName("likes")]
    public long Likes { get; init; }

    [JsonPropertyName("isExplicit")]
    public bool IsExplicit { get; init; }

    [JsonPropertyName("genres")]
    public List<ShortGenreDto> Genres { get; init; } = null!;

    [JsonPropertyName("allowedRegions")]
    public List<ShortRegionDto> AllowedRegions { get; init; } = null!;

    public static ShortSongDto FromEntity(SongEntity song) =>
        new ShortSongDto
        {
            Id = song.Id,
            Title = song.Title,
            Artists = song.Artists
                .Select(x => ShortSongArtistDto.FromEntity(x.Artist, x.MainArtist))
                .ToList(),
            DurationMs = song.DurationMs,
            Likes = song.Likes,
            IsExplicit = song.Explicit,
            Genres = song.Genres
                .Select(ShortGenreDto.FromEntity)
                .ToList(),
            AllowedRegions = song.AllowedRegions
                .Select(ShortRegionDto.FromEntity)
                .ToList()
        };
}