namespace Lumindex.Application.Common.Interfaces;

/// <summary>
/// Ambient accessor for the authenticated caller. Implemented in the API layer over the
/// current <c>HttpContext</c>; used by handlers to scope data access per user.
/// </summary>
public interface ICurrentUser
{
    Guid? Id { get; }

    bool IsAuthenticated { get; }
}
