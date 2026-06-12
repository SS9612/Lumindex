using Lumindex.Application.Authentication.Models;
using MediatR;

namespace Lumindex.Application.Authentication.Commands.Register;

public sealed record RegisterCommand(string Email, string DisplayName, string Password)
    : IRequest<AuthenticationResult>;
