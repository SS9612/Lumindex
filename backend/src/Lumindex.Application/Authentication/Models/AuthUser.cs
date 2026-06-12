namespace Lumindex.Application.Authentication.Models;

public sealed record AuthUser(Guid Id, string Email, string DisplayName);
