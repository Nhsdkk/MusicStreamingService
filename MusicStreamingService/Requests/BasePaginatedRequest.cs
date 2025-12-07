using System.Text.Json.Serialization;
using MusicStreamingService.Features.Songs;

namespace MusicStreamingService.Requests;

public abstract record BasePaginatedRequest
{
    [JsonPropertyName("itemsPerPage")]
    public int ItemsPerPage { get; set; } = 10;

    [JsonPropertyName("page")]
    public int Page { get; set; } = 0;
}