using System.Text.Json.Serialization;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Features.Albums;
using MusicStreamingService.Features.Genres;
using MusicStreamingService.Features.Region;
using MusicStreamingService.Features.Users;

namespace MusicStreamingService.Features.Songs;

public class ShortAlbumSongDto
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

    [JsonPropertyName("allowedRegions")]
    public List<RegionDto> AllowedRegions { get; init; } = null!;
    
    [JsonPropertyName("albumPosition")]
    public long AlbumPosition { get; init; }
    
    [JsonPropertyName("titleTrack")]
    public bool IsTitleTrack { get; init; }
    
    [JsonPropertyName("songUrl")]
    public string? SongUrl { get; init; }
    
    public static ShortAlbumSongDto FromEntity(
        SongEntity song,
        string? songUrl) =>
        new ShortAlbumSongDto
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
                .Select(GenreDto.FromEntity)
                .ToList(),
            AllowedRegions = song.AllowedRegions
                .Select(RegionDto.FromEntity)
                .ToList(),
            AlbumPosition = song.AlbumPosition,
            IsTitleTrack = song.IsTitleTrack,
            SongUrl = songUrl
        };
}