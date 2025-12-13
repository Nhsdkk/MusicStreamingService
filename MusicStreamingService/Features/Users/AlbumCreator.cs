using System.Text.Json.Serialization;

namespace MusicStreamingService.Features.Users;

public class AlbumCreator
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }
            
    [JsonPropertyName("username")]
    public string Username { get; init; } = null!;
    
    public static AlbumCreator FromEntity(Data.Entities.UserEntity user) =>
        new AlbumCreator
        {
            Id = user.Id,
            Username = user.Username
        };
}