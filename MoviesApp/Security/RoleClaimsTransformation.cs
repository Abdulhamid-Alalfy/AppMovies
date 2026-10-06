using System.Security.Claims;
using Domin.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace MoviesApp.Security;

public sealed class RoleClaimsTransformation : IClaimsTransformation
{
    private readonly UserManager<AppUser> _userManager;

    public RoleClaimsTransformation(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return principal;
        }

        var user = await _userManager.GetUserAsync(principal);
        if (user == null)
        {
            return principal;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var currentRoles = identity.FindAll(identity.RoleClaimType)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (currentRoles.SetEquals(roles))
        {
            return principal;
        }

        foreach (var claim in identity.FindAll(identity.RoleClaimType).ToList())
        {
            identity.RemoveClaim(claim);
        }

        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(identity.RoleClaimType, role));
        }

        return principal;
    }
}
