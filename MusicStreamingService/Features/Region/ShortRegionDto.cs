using System.Text.Json.Serialization;
using MusicStreamingService.Data.Entities;

namespace MusicStreamingService.Features.Region;

public sealed record ShortRegionDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = null!;

    public static ShortRegionDto FromEntity(RegionEntity region) =>
        new ShortRegionDto
        {
            Id = region.Id,
            Title = region.Title
        };
}