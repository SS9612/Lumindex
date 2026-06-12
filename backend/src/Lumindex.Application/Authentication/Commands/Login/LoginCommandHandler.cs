using Lumindex.Application.Authentication.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lumindex.Application.Authentication.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthenticationResult>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IIdentityService identityService,
        IJwtTokenGenerator tokenGenerator,
        ILogger<LoginCommandHandler> logger)
    {
        _identityService = identityService;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
    }

    public async Task<AuthenticationResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _identityService.ValidateCredentialsAsync(email, request.Password, cancellationToken);
        if (user is null)
        {
            _logger.LogInformation("Failed login attempt for {Email}", email);
            return AuthenticationResult.Failure(
                AuthError.InvalidCredentials,
                "Invalid email or password.");
        }

        var token = _tokenGenerator.Generate(user);
        return AuthenticationResult.Success(token, user);
    }
}
