using System.Text.Json.Serialization;
using MusicStreamingService.Data.Entities;

namespace MusicStreamingService.Features.Region;

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