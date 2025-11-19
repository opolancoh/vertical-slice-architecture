using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using VerticalSliceApp.Api.Common.Extensions;
using VerticalSliceApp.Api.Features.Users;

namespace VerticalSliceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest request,
        [FromServices] CreateUserService service,
        [FromServices] IValidator<CreateUserRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(validationResult.ToDictionary());
        }

        var response = await service.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(
        Guid id,
        [FromServices] GetUserService service,
        CancellationToken cancellationToken)
    {
        var user = await service.ExecuteAsync(id, cancellationToken);

        return user is not null
            ? Ok(user)
            : NotFound();
    }
}
