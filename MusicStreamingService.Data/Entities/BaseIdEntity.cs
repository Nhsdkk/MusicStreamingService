using MusicStreamingService.Data.Entities.Configurations;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities;

public record BaseIdEntity : IId<Guid>, ICreationTime
{
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}