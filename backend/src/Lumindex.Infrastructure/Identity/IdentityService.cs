using Lumindex.Application.Authentication;
using Lumindex.Application.Authentication.Models;
using Lumindex.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Lumindex.Infrastructure.Identity;

/// <summary>
/// Adapts ASP.NET Core Identity's <see cref="UserManager{TUser}"/> to the Application-layer
/// <see cref="IIdentityService"/> contract.
/// </summary>
public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;

    public IdentityService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<CreateUserResult> CreateUserAsync(
        string email,
        string displayName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return new CreateUserResult(false, true, null, new[] { "An account with this email already exists." });
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            DisplayName = displayName,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            // A unique-index race can still surface a duplicate here.
            var duplicate = result.Errors.Any(e =>
                e.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase));

            return new CreateUserResult(
                false,
                duplicate,
                null,
                result.Errors.Select(e => e.Description).ToArray());
        }

        return new CreateUserResult(true, false, ToAuthUser(user), Array.Empty<string>());
    }

    public async Task<AuthUser?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var isValid = await _userManager.CheckPasswordAsync(user, password);
        return isValid ? ToAuthUser(user) : null;
    }

    public async Task<AuthUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : ToAuthUser(user);
    }

    private static AuthUser ToAuthUser(AppUser user) =>
        new(user.Id, user.Email!, user.DisplayName);
}
