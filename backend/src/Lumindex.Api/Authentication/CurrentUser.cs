using System.Security.Claims;
using Lumindex.Application.Common.Interfaces;

namespace Lumindex.Api.Authentication;

/// <summary>
/// Resolves the authenticated caller from the current <see cref="HttpContext"/>. The JWT
/// <c>sub</c> claim is mapped to <see cref="ClaimTypes.NameIdentifier"/> by the bearer handler.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public Guid? Id
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var value = principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? principal?.FindFirstValue("sub");

            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
