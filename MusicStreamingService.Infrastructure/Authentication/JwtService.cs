using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MusicStreamingService.Infrastructure.Result;

namespace MusicStreamingService.Infrastructure.Authentication;

/// <summary>
/// Jwt service for jwt generation and validation 
/// </summary>
/// <typeparam name="T">Type of claim, that will be stored in jwt</typeparam>
public interface IJwtService<in T> where T : IClaimConvertable
{
    /// <summary>
    /// Get jwt token pair (access and refresh tokens) using claims
    /// </summary>
    /// <param name="data">Claims</param>
    /// <returns>Pair of tokens (access and refresh tokens)</returns>
    public (string accessToken, string refreshToken) GetPair(T data);

    /// <summary>
    /// Refresh access token
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <returns>Either refreshed access token, or <see cref="JwtValidationException"/>, that occured while validating refresh token</returns>
    public Result<string, JwtValidationException> RefreshAccessToken(string refreshToken);

    /// <summary>
    /// Validate token and get claims from it
    /// </summary>
    /// <param name="token">Token to be validated</param>
    /// <returns>Either a collection of claims or <see cref="JwtValidationException"/>, that occured while validating token</returns>
    public Result<IEnumerable<Claim>, JwtValidationException> ValidateToken(string token);
    
    /// <summary>
    /// Get token validation parameters for current configuration
    /// </summary>
    /// <returns>Token validation parameters</returns>
    public TokenValidationParameters GetTokenValidationParameters();
}

/// <inheritdoc/>
public class JwtService<T> : IJwtService<T> where T : IClaimConvertable
{
    private readonly JwtConfiguration _configuration;

    public JwtService(IOptions<JwtConfiguration> options)
    {
        _configuration = options.Value;
    }
    
    public JwtService(JwtConfiguration config)
    {
        _configuration = config;
    }

    public (string accessToken, string refreshToken) GetPair(T data)
    {
        var accessToken = GetToken(GetClaims(data), _configuration.AccessTokenExpiration);
        var refreshToken = GetToken(GetClaims(data), _configuration.RefreshTokenExpiration);

        return (accessToken, refreshToken);
    }

    public Result<string, JwtValidationException> RefreshAccessToken(string refreshToken)
    {
        var claims = ValidateToken(refreshToken);

        Result<string, JwtValidationException> result = null!;
        claims.Switch(
            c => result = GetToken(c, _configuration.AccessTokenExpiration),
            ex => result = ex
        );

        return result;
    }

    public Result<IEnumerable<Claim>, JwtValidationException> ValidateToken(string token)
    {
        var validationParameters = GetTokenValidationParameters();

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(
                token,
                validationParameters,
                out _
            );

            return principal.Claims.ToList();
        }
        catch (SecurityTokenArgumentException ex)
        {
            return new JwtValidationException("Token is malformed", ex);
        }
        catch (SecurityTokenException ex)
        {
            return new JwtValidationException("Token decryption failed", ex);
        }
    }

    public TokenValidationParameters GetTokenValidationParameters() => new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = _configuration.Issuer,
        ValidateAudience = true,
        ValidAudience = _configuration.Audience,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.SecretKey)),
    };

    private IEnumerable<Claim> GetClaims(T data)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Name, data.GetUsername()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sid, data.GetId().ToString())
        };

        claims.AddRange(data.GetPermissions().Select(x => new Claim(ClaimTypes.Role, x)));
        return claims;
    }

    private string GetToken(IEnumerable<Claim> claims, TimeSpan expiration)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _configuration.Issuer,
            _configuration.Audience,
            claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow + expiration,
            credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}