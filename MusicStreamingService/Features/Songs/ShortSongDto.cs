using System.Text.Json.Serialization;
using MusicStreamingService.Data.Entities;

namespace MusicStreamingService.Features.Songs;

public sealed record ShortSongDto
{
    public sealed record ShortArtistInfoDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("username")]
        public string Username { get; init; } = null!;

        [JsonPropertyName("mainArtist")]
        public bool MainArtist { get; init; }

        public static ShortArtistInfoDto FromEntity(UserEntity artist, bool mainArtist) =>
            new ShortArtistInfoDto
            {
                Id = artist.Id,
                Username = artist.Username,
                MainArtist = mainArtist
            };
    }

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

    public sealed record RegionDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("title")]
        public string Title { get; init; } = null!;

        public static RegionDto FromEntity(RegionEntity region) =>
            new RegionDto
            {
                Id = region.Id,
                Title = region.Title
            };
    }

    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = null!;

    [JsonPropertyName("artists")]
    public List<ShortArtistInfoDto> Artists { get; init; } = null!;

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

    public static ShortSongDto FromEntity(SongEntity song) =>
        new ShortSongDto
        {
            Id = song.Id,
            Title = song.Title,
            Artists = song.Artists
                .Select(x => ShortArtistInfoDto.FromEntity(x.Artist, x.MainArtist))
                .ToList(),
            DurationMs = song.DurationMs,
            Likes = song.Likes,
            IsExplicit = song.Explicit,
            Genres = song.Genres
                .Select(GenreDto.FromEntity)
                .ToList(),
            AllowedRegions = song.AllowedRegions
                .Select(RegionDto.FromEntity)
                .ToList()
        };
}