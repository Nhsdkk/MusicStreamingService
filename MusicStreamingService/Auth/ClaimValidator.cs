using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Infrastructure.Authentication;

namespace MusicStreamingService.Auth;

public sealed class ClaimValidator : IClaimValidator
{
    private readonly MusicStreamingContext _context;
    
    public ClaimValidator(MusicStreamingContext context)
    {
        _context = context;
    }
    
    public async Task<bool> Validate(IClaimConvertable claims, CancellationToken cancellationToken)
    {
        var userId = claims.GetId();
        var user = await _context.Users
            .AsNoTracking()
            .Include(x => x.Region)
            .Include(x => x.Roles)
            .ThenInclude(x => x.Permissions)
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null || user.Disabled)
        {
            return false;
        }

        var permissions = user.Roles
            .SelectMany(x => x.Permissions)
            .Select(x => x.Title)
            .ToList();
        
        var claimPermissions = claims.GetPermissions().ToList();
        var allRolesMatch = permissions.All(r => claimPermissions.Any(cr => cr == r));
            
        if (claimPermissions.Count != permissions.Count || !allRolesMatch)
        {
            return false;
        }

        var claimRegion = claims.GetRegion();
        if (user.Region.Id != claimRegion.Id || user.Region.Title != claimRegion.Title)
        {
            return false;
        }

        return true;
    }
}