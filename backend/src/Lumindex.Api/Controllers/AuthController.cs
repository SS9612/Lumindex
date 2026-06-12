using Lumindex.Api.Contracts.Auth;
using Lumindex.Application.Authentication.Commands.Login;
using Lumindex.Application.Authentication.Commands.Register;
using Lumindex.Application.Authentication.Models;
using Lumindex.Application.Authentication.Queries.GetCurrentUser;
using Lumindex.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumindex.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ICurrentUser _currentUser;

    public AuthController(ISender mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>Create a new account and return a JWT access token.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RegisterCommand(request.Email, request.DisplayName, request.Password),
            cancellationToken);

        if (result.Succeeded)
        {
            return StatusCode(StatusCodes.Status201Created, ToResponse(result));
        }

        return MapFailure(result);
    }

    /// <summary>Authenticate an existing account and return a JWT access token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        return result.Succeeded ? Ok(ToResponse(result)) : MapFailure(result);
    }

    /// <summary>Return the profile of the currently authenticated user.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        if (_currentUser.Id is not { } userId)
        {
            return Unauthorized();
        }

        var user = await _mediator.Send(new GetCurrentUserQuery(userId), cancellationToken);
        return user is null ? Unauthorized() : Ok(new UserResponse(user.Id, user.Email, user.DisplayName));
    }

    private static AuthResponse ToResponse(AuthenticationResult result)
    {
        var user = result.User!;
        return new AuthResponse(
            result.AccessToken!,
            result.ExpiresAt!.Value,
            new UserResponse(user.Id, user.Email, user.DisplayName));
    }

    private IActionResult MapFailure(AuthenticationResult result)
    {
        var detail = result.ErrorMessages.Count > 0
            ? string.Join(" ", result.ErrorMessages)
            : "Authentication failed.";

        return result.Error switch
        {
            AuthError.EmailAlreadyInUse => Problem(detail, statusCode: StatusCodes.Status409Conflict, title: "Email already in use"),
            AuthError.InvalidCredentials => Problem(detail, statusCode: StatusCodes.Status401Unauthorized, title: "Invalid credentials"),
            _ => Problem(detail, statusCode: StatusCodes.Status400BadRequest, title: "Registration failed"),
        };
    }
}
