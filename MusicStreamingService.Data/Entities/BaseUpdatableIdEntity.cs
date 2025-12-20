using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities;

public record BaseUpdatableIdEntity : BaseIdEntity, IModificationTime
{
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
};