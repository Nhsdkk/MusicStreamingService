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
public interface IJwtService<T> where T : IClaimConvertable, new()
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
    /// <param name="cancellationToken"></param>
    /// <returns>Either refreshed access token, or <see cref="JwtValidationException"/>, that occured while validating refresh token</returns>
    public Task<Result<string, JwtValidationException>> RefreshAccessToken(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate token and get claims from it
    /// </summary>
    /// <param name="token">Token to be validated</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Either a collection of claims or <see cref="JwtValidationException"/>, that occured while validating token</returns>
    public Task<Result<T, JwtValidationException>> ValidateToken(string token, CancellationToken cancellationToken = default);
}

/// <inheritdoc/>
public class JwtService<T> : IJwtService<T> where T : IClaimConvertable, new()
{
    private readonly JwtConfiguration _configuration;
    private readonly IClaimValidator _validator;

    public JwtService(IOptions<JwtConfiguration> options, IClaimValidator claimsValidator)
    {
        _configuration = options.Value;
        _validator = claimsValidator;
    }
    
    public JwtService(JwtConfiguration config, IClaimValidator validator)
    {
        _configuration = config;
        _validator = validator;
    }

    public (string accessToken, string refreshToken) GetPair(T data)
    {
        var accessToken = GetToken(GetClaims(data), _configuration.AccessTokenExpiration);
        var refreshToken = GetToken(GetClaims(data), _configuration.RefreshTokenExpiration);

        return (accessToken, refreshToken);
    }

    public async Task<Result<string, JwtValidationException>> RefreshAccessToken(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var claims = await ValidateToken(refreshToken, cancellationToken);

        Result<string, JwtValidationException> result = null!;
        claims.Switch(
            c => result = GetToken(GetClaims(c), _configuration.AccessTokenExpiration),
            ex => result = ex
        );

        return result;
    }

    public async Task<Result<T, JwtValidationException>> ValidateToken(
        string token,
        CancellationToken cancellationToken = default)
    {
        var validationParameters = _configuration.GetTokenValidationParameters();

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(
                token,
                validationParameters,
                out _
            );

            var claimsResult = new T().FromClaims(principal.Claims.ToList());
            if (claimsResult.IsT1)
            {
                return new JwtValidationException("Failed to parse claims from token");
            }

            var claims = claimsResult.AsT0!;
            
            var validatorResult = await _validator.Validate(claims, cancellationToken);
            if (!validatorResult)
            {
                return new JwtValidationException("Token claims validation failed");
            }

            return (T)claims;
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

    private IEnumerable<Claim> GetClaims(T data)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Name, data.GetUsername()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sid, data.GetId().ToString()),
            new Claim(CustomClaimTypes.RegionsClaimType, System.Text.Json.JsonSerializer.Serialize(data.GetRegion()))
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