using System.Text.Json.Serialization;

namespace MusicStreamingService.Infrastructure.DateUtils;

public class DateRange
{
    [JsonPropertyName("start")]
    public DateTime? Start { get; init; }
    
    [JsonPropertyName("end")]
    public DateTime? End { get; init; }

    public bool InRange(DateTime dateTime)
    {
        if (Start.HasValue && dateTime < Start.Value)
        {
            return false;
        }

        if (End.HasValue && dateTime > End.Value)
        {
            return false;
        }

        return true;
    }
}