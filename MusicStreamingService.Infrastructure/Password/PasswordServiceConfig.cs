namespace MusicStreamingService.Infrastructure.Password;

public sealed record PasswordServiceConfig
{
    /// <summary>
    /// Salt size in bytes
    /// </summary>
    public int SaltSize { get; init; }
    
    /// <summary>
    /// Number of iterations of the algorithm
    /// </summary>
    public int IterationsCount { get; init; }
    
    /// <summary>
    /// Desired size of the hash
    /// </summary>
    public int NumBytesRequested { get; init; }
};