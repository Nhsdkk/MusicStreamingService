namespace MusicStreamingService.Infrastructure.Password;

public sealed record PasswordServiceConfig
{
    /// <summary>
    /// Salt size in bytes
    /// </summary>
    public int SaltSize { get; set; }
    
    /// <summary>
    /// Number of iterations of the algorithm
    /// </summary>
    public int IterationsCount { get; set; }
    
    /// <summary>
    /// Desired size of the hash
    /// </summary>
    public int NumBytesRequested { get; set; }
    
};