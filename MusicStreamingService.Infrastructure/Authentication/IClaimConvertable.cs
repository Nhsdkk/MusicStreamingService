namespace MusicStreamingService.Infrastructure.Authentication;

public interface IClaimConvertable
{
    /// <summary>
    /// Get permissions to store in jwt
    /// </summary>
    /// <returns>Permissions, that will be stored in jwt</returns>
    public IEnumerable<string> GetPermissions();
    
    /// <summary>
    /// Get username to store in jwt
    /// </summary>
    /// <returns>Username, that will be stored in jwt</returns>
    public string GetUsername();

    /// <summary>
    /// Get id of the user to store in jwt
    /// </summary>
    /// <returns>User's id, that will be stored in jwt</returns>
    public Guid GetId();
}