using DocuMind.Application.Authentication.Models;
using MediatR;

namespace DocuMind.Application.Authentication.Commands.Register;

public sealed record RegisterCommand(string Email, string DisplayName, string Password)
    : IRequest<AuthenticationResult>;
