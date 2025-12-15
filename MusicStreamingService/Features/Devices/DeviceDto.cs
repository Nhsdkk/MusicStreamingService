using System.Text.Json.Serialization;

namespace MusicStreamingService.Features.Devices;

public class DeviceDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;   
}