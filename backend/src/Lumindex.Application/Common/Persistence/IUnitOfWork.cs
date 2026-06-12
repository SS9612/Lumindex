namespace Lumindex.Application.Common.Persistence;

/// <summary>
/// Commits changes tracked across the repositories within a single logical operation.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
