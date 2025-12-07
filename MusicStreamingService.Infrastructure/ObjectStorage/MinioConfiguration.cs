namespace MusicStreamingService.Infrastructure.ObjectStorage;

public sealed record MinioConfiguration
{
    public string Endpoint { get; init; } = null!;

    public string AccessKey { get; init; } = null!;

    public string SecretKey { get; init; } = null!;

    public int ExpireTimeInSeconds { get; init; }
    
    public bool UseSsl { get; init; }
}