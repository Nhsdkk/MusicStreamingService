using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MusicStreamingService.Data.Entities;

public enum StreamingEventType
{
    /// <summary>
    /// Playback has started
    /// </summary>
    Play,
    
    /// <summary>
    /// Playback is continuing by requesting new batch 
    /// </summary>
    Continue,
    
    /// <summary>
    /// Playback paused
    /// </summary>
    Pause,
    
    /// <summary>
    /// Playback point moved
    /// </summary>
    Seek,
}

internal sealed class StreamingEventTypeConverter : ValueConverter<StreamingEventType, string>
{
    public StreamingEventTypeConverter()
        : base(
            v => v.ToString(),
            v => Enum.Parse<StreamingEventType>(v))
    {
    }
}