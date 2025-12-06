using System.Text.Json.Serialization;

namespace MusicStreamingService.Infrastructure.Authentication;

public sealed record RegionClaim
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }
    
    [JsonPropertyName("title")]
    public string Title { get; init; } = null!;
}