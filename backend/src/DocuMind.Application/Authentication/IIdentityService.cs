using DocuMind.Application.Authentication.Models;

namespace DocuMind.Application.Authentication;

/// <summary>
/// Abstraction over ASP.NET Core Identity's <c>UserManager</c> so the Application layer stays
/// free of Identity types. Implemented in the Infrastructure layer.
/// </summary>
public interface IIdentityService
{
    Task<CreateUserResult> CreateUserAsync(string email, string displayName, string password, CancellationToken cancellationToken = default);

    Task<AuthUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<AuthUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public sealed record CreateUserResult(
    bool Succeeded,
    bool EmailAlreadyInUse,
    AuthUser? User,
    IReadOnlyList<string> Errors);
