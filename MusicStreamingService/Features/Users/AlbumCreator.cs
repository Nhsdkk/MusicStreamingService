using System.Text.Json.Serialization;

namespace MusicStreamingService.Features.Users;

public class ShortAlbumCreatorDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }
            
    [JsonPropertyName("username")]
    public string Username { get; init; } = null!;
    
    public static ShortAlbumCreatorDto FromEntity(Data.Entities.UserEntity user) =>
        new ShortAlbumCreatorDto
        {
            Id = user.Id,
            Username = user.Username
        };
}