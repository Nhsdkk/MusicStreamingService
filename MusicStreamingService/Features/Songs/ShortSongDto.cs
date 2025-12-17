using System.Text.Json.Serialization;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Features.Albums;
using MusicStreamingService.Features.Genres;
using MusicStreamingService.Features.Region;
using MusicStreamingService.Features.Users;
using MusicStreamingService.Infrastructure.Authentication;

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
    public List<GenreDto> Genres { get; init; } = null!;

    [JsonPropertyName("allowedInUserRegion")]
    public bool AllowedInUserRegion { get; init; }
    
    [JsonPropertyName("album")]
    public ShortAlbumDto Album { get; init; } = null!;

    public static ShortSongDto FromEntity(
        SongEntity song,
        string? albumArtworkUrl,
        RegionClaim userRegion) =>
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
            Album = ShortAlbumDto.FromEntity(song.Album, albumArtworkUrl),
            Genres = song.Genres
                .Select(GenreDto.FromEntity)
                .ToList(),
            AllowedInUserRegion = song.AllowedRegions
                .Any(x => x.Id == userRegion.Id)
        };
}