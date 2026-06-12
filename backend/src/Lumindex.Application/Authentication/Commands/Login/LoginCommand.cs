using Lumindex.Application.Authentication.Models;
using MediatR;

namespace Lumindex.Application.Authentication.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthenticationResult>;
