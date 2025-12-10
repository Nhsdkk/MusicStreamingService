namespace MusicStreamingService.Infrastructure.Authentication;

public sealed class UserClaims
{
    public List<string> Permissions { get; init; } = null!;

    public string Username { get; init; } = null!;
    
    public Guid Id { get; init; }

    public RegionClaim Region { get; init; } = null!;
}