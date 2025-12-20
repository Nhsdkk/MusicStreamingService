using System.Text.Json.Serialization;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Features.Users;

namespace MusicStreamingService.Features.Playlists;

public sealed record ShortPlaylistDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = null!;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("creator")]
    public ShortUserDto Creator { get; init; } = null!;

    [JsonPropertyName("accessType")]
    public PlaylistAccessType AccessType { get; init; }

    [JsonPropertyName("likes")]
    public long Likes { get; init; }

    public static ShortPlaylistDto FromEntity(
        PlaylistEntity playlist) =>
        new ShortPlaylistDto
        {
            Id = playlist.Id,
            Title = playlist.Title,
            Description = playlist.Description,
            Creator = ShortUserDto.FromEntity(playlist.Creator),
            AccessType = playlist.AccessType,
            Likes = playlist.Likes,
        };
}