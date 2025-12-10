using System.Security.Claims;
using MusicStreamingService.Infrastructure.Result;

namespace MusicStreamingService.Infrastructure.Authentication;

public interface IClaimValidator<T>
{
    /// <summary>
    /// Validate claims and return the result of the validation
    /// </summary>
    /// <param name="claims">User's claims</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<Exception?> Validate(T claims, CancellationToken cancellationToken);
}