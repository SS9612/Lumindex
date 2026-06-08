using DocuMind.Application.Authentication.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocuMind.Application.Authentication.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthenticationResult>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IJwtTokenGenerator tokenGenerator,
        ILogger<RegisterCommandHandler> logger)
    {
        _identityService = identityService;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
    }

    public async Task<AuthenticationResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var result = await _identityService.CreateUserAsync(
            email,
            request.DisplayName.Trim(),
            request.Password,
            cancellationToken);

        if (result.EmailAlreadyInUse)
        {
            return AuthenticationResult.Failure(
                AuthError.EmailAlreadyInUse,
                "An account with this email already exists.");
        }

        if (!result.Succeeded || result.User is null)
        {
            _logger.LogWarning("Registration failed for {Email}: {Errors}", email, string.Join("; ", result.Errors));
            return AuthenticationResult.Failure(AuthError.RegistrationFailed, result.Errors.ToArray());
        }

        _logger.LogInformation("Registered new user {UserId}", result.User.Id);

        var token = _tokenGenerator.Generate(result.User);
        return AuthenticationResult.Success(token, result.User);
    }
}
