namespace MusicStreamingService.Data;

public sealed class PsqlConfiguration
{
    /// <summary>
    /// Host of the database
    /// </summary>
    public string Host { get; set; } = null!;
    
    /// <summary>
    /// Port for the database
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Service account's username
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Service account's password
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// Database, to which you will be connected
    /// </summary>
    public string Database { get; set; } = null!;

    /// <summary>
    /// Get database connection string 
    /// </summary>
    /// <returns>Database connection string</returns>
    public string GetConnectionString() => $"Username={Username}; Password={Password}; Host={Host}; Port={Port}; Database={Database};";
}