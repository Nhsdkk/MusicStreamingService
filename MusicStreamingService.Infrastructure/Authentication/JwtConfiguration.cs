using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MusicStreamingService.Infrastructure.Authentication;

public sealed record JwtConfiguration
{
    /// <summary>
    /// Key to sign jwt using HMACSHA256 algorithm
    /// </summary>
    public string SecretKey { get; init; } = null!;

    /// <summary>
    /// Token issuer
    /// </summary>
    public string Issuer { get; init; } = null!;

    /// <summary>
    /// Valid audience
    /// </summary>
    public string Audience { get; init; } = null!;
    
    /// <summary>
    /// Expiration time of the access token
    /// </summary>
    public TimeSpan AccessTokenExpiration { get; init; }
    
    /// <summary>
    /// Expiration time of the refresh token
    /// </summary>
    public TimeSpan RefreshTokenExpiration { get; init; }
    
    public TokenValidationParameters GetTokenValidationParameters() => new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = Issuer,
        ValidateAudience = true,
        ValidAudience = Audience,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)),
    };
}