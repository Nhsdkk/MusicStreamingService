using System.Text.Json.Serialization;

namespace MusicStreamingService.Data.Utils;

public sealed class DateRange
{
    [JsonPropertyName("start")]
    public DateOnly? Start { get; init; }
    
    [JsonPropertyName("end")]
    public DateOnly? End { get; init; }
}