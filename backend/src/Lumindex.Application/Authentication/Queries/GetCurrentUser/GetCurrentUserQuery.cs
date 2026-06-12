using Lumindex.Application.Authentication.Models;
using MediatR;

namespace Lumindex.Application.Authentication.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<AuthUser?>;
