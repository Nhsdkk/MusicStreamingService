using System.Text.Json.Serialization;

namespace MusicStreamingService.Features.Users;

public class ShortUserDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }
            
    [JsonPropertyName("username")]
    public string Username { get; init; } = null!;
    
    public static ShortUserDto FromEntity(Data.Entities.UserEntity user) =>
        new ShortUserDto
        {
            Id = user.Id,
            Username = user.Username
        };
}