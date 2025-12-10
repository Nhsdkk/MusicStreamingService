using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Infrastructure.Authentication;

namespace MusicStreamingService.Auth;

public sealed class ClaimValidator : IClaimValidator<UserClaims>
{
    private readonly MusicStreamingContext _context;
    private readonly IClaimConverter<UserClaims> _claimConverter;

    public ClaimValidator(
        MusicStreamingContext context, 
        IClaimConverter<UserClaims> claimConverter)
    {
        _context = context;
        _claimConverter = claimConverter;
    }

    public async Task<Exception?> Validate(UserClaims claims, CancellationToken cancellationToken)
    {
        var userId = claims.Id;
        var user = await _context.Users
            .AsNoTracking()
            .Include(x => x.Region)
            .Include(x => x.Roles)
            .ThenInclude(x => x.Permissions)
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null || user.Disabled)
        {
            return new Exception("Invalid claims");
        }

        var permissions = user.Roles
            .SelectMany(x => x.Permissions)
            .Select(x => x.Title)
            .ToHashSet();

        var claimPermissions = claims.Permissions.ToHashSet();
        var allPermissionsMatch = claimPermissions.SetEquals(permissions);

        if (!allPermissionsMatch)
        {
            return new Exception("Invalid claims");
        }

        var claimRegion = claims.Region;
        if (user.Region.Id != claimRegion.Id || user.Region.Title != claimRegion.Title)
        {
            return new Exception("Invalid claims");
        }

        return null;
    }
}