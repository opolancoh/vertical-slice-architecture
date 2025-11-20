using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using VerticalSliceApp.Api.Common;
using VerticalSliceApp.Api.Common.Extensions;
using VerticalSliceApp.Api.Features.Users;

namespace VerticalSliceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<GetAllUsersResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers(
        [FromServices] GetAllUsersService service,
        CancellationToken cancellationToken)
    {
        var users = await service.ExecuteAsync(cancellationToken);
        return Ok(ApiResponse<List<GetAllUsersResponse>>.Ok(users));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(
        Guid id,
        [FromServices] GetUserService service,
        CancellationToken cancellationToken)
    {
        var user = await service.ExecuteAsync(id, cancellationToken);

        return user is not null
            ? Ok(ApiResponse<GetUserResponse>.Ok(user))
            : NotFound(ApiResponse.Fail("User not found"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest request,
        [FromServices] CreateUserService service,
        [FromServices] IValidator<CreateUserRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse.Fail("Validation failed", errors));
        }

        var response = await service.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetUser),
            new { id = response.Id },
            ApiResponse<UserResponse>.Ok(response, "User created successfully"));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        [FromServices] UpdateUserService service,
        [FromServices] IValidator<UpdateUserRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse.Fail("Validation failed", errors));
        }

        var response = await service.ExecuteAsync(id, request, cancellationToken);

        return response is not null
            ? Ok(ApiResponse<UserResponse>.Ok(response, "User updated successfully"))
            : NotFound(ApiResponse.Fail("User not found"));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(
        Guid id,
        [FromServices] DeleteUserService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(id, cancellationToken);

        return result
            ? Ok(ApiResponse.Ok("User deleted successfully"))
            : NotFound(ApiResponse.Fail("User not found"));
    }
}
