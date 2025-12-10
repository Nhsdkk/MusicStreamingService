namespace MusicStreamingService.Infrastructure.Authentication;

public interface IClaimValidator
{
    /// <summary>
    /// Validate claims and return the result of the validation
    /// </summary>
    /// <param name="claims">User's claims</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<bool> Validate(IClaimConvertable claims, CancellationToken cancellationToken);
}