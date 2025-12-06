using System.Text.Json.Serialization;

namespace MusicStreamingService.Responses;

public abstract record BasePaginatedResponse
{
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; init; }

    [JsonPropertyName("itemsPerPage")]
    public int ItemsPerPage { get; init; }

    [JsonPropertyName("itemCount")]
    public int ItemCount { get; init; }

    [JsonPropertyName("page")]
    public int Page { get; init; }
}