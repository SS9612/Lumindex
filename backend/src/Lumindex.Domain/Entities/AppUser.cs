using Microsoft.AspNetCore.Identity;

namespace Lumindex.Domain.Entities;

/// <summary>
/// Application user backed by ASP.NET Core Identity. Uses a <see cref="Guid"/> primary key so
/// that owned resources (documents, conversations) can reference it via their <c>OwnerId</c>.
/// </summary>
public sealed class AppUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
