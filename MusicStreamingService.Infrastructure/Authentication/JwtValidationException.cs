namespace MusicStreamingService.Infrastructure.Authentication;

/// <summary>
/// Exception, while validating and deserializing jwt token 
/// </summary>
public sealed class JwtValidationException : Exception
{
    public JwtValidationException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}