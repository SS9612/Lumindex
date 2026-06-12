namespace Lumindex.Application.Authentication.Models;

public enum AuthError
{
    None = 0,
    EmailAlreadyInUse,
    InvalidCredentials,
    RegistrationFailed,
}

/// <summary>
/// Outcome of a register/login use case. Carries either the issued token and user, or a
/// categorised failure that the API maps to the appropriate HTTP status code.
/// </summary>
public sealed record AuthenticationResult
{
    public bool Succeeded { get; init; }
    public AuthError Error { get; init; } = AuthError.None;
    public IReadOnlyList<string> ErrorMessages { get; init; } = Array.Empty<string>();
    public string? AccessToken { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public AuthUser? User { get; init; }

    public static AuthenticationResult Success(AuthToken token, AuthUser user) => new()
    {
        Succeeded = true,
        AccessToken = token.AccessToken,
        ExpiresAt = token.ExpiresAt,
        User = user,
    };

    public static AuthenticationResult Failure(AuthError error, params string[] messages) => new()
    {
        Succeeded = false,
        Error = error,
        ErrorMessages = messages,
    };
}
