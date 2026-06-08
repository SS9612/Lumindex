using DocuMind.Application.Authentication.Models;
using MediatR;

namespace DocuMind.Application.Authentication.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, AuthUser?>
{
    private readonly IIdentityService _identityService;

    public GetCurrentUserQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<AuthUser?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken) =>
        _identityService.FindByIdAsync(request.UserId, cancellationToken);
}
