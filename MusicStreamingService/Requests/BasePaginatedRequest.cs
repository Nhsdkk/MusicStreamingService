using System.Text.Json.Serialization;

namespace MusicStreamingService.Requests;

public abstract record BasePaginatedRequest
{
    [JsonPropertyName("itemsPerPage")]
    public int ItemsPerPage { get; init; } = 10;

    [JsonPropertyName("page")]
    public int Page { get; init; } = 0;
}