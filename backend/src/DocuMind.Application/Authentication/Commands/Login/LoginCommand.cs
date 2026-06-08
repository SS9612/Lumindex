using DocuMind.Application.Authentication.Models;
using MediatR;

namespace DocuMind.Application.Authentication.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthenticationResult>;
