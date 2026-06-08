using DocuMind.Application.Authentication.Models;
using MediatR;

namespace DocuMind.Application.Authentication.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<AuthUser?>;
