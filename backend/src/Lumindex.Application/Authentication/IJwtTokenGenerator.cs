using Lumindex.Application.Authentication.Models;

namespace Lumindex.Application.Authentication;

/// <summary>
/// Issues signed JWT access tokens for an authenticated user. Implemented in the API layer
/// where the JWT signing configuration lives.
/// </summary>
public interface IJwtTokenGenerator
{
    AuthToken Generate(AuthUser user);
}
