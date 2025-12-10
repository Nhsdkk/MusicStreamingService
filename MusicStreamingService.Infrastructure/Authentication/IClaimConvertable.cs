using System.Security.Claims;

namespace MusicStreamingService.Infrastructure.Authentication;

public interface IClaimConvertable
{
    /// <summary>
    /// Get permissions
    /// </summary>
    /// <returns>Permissions</returns>
    public IEnumerable<string> GetPermissions();
    
    /// <summary>
    /// Get username
    /// </summary>
    /// <returns>Username</returns>
    public string GetUsername();

    /// <summary>
    /// Get id of the user
    /// </summary>
    /// <returns>User's id</returns>
    public Guid GetId();

    /// <summary>
    /// Get user's region
    /// </summary>
    /// <returns>User's region,</returns>
    public RegionClaim GetRegion();
    
    /// <summary>
    /// Get user's birth date
    /// </summary>
    /// <returns>User's birthdate</returns>
    public DateTime GetBirthDate();

    /// <summary>
    /// Convert to claims
    /// </summary>
    /// <returns>Jwt claims</returns>
    public List<Claim> ToClaims();
}