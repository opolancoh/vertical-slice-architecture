using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using VerticalSliceApp.Api.Common;
using VerticalSliceApp.Api.Common.Extensions;
using VerticalSliceApp.Api.Features.Customers;

namespace VerticalSliceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<GetAllCustomersResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCustomers(
        [FromServices] GetAllCustomersService service,
        CancellationToken cancellationToken)
    {
        var customers = await service.ExecuteAsync(cancellationToken);
        return Ok(ApiResponse<List<GetAllCustomersResponse>>.Ok(customers));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetCustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomer(
        Guid id,
        [FromServices] GetCustomerService service,
        CancellationToken cancellationToken)
    {
        var customer = await service.ExecuteAsync(id, cancellationToken);

        return customer is not null
            ? Ok(ApiResponse<GetCustomerResponse>.Ok(customer))
            : NotFound(ApiResponse.Fail("Customer not found"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        [FromServices] CreateCustomerService service,
        [FromServices] IValidator<CreateCustomerRequest> validator,
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
            nameof(GetCustomer),
            new { id = response.Id },
            ApiResponse<CustomerResponse>.Ok(response, "Customer created successfully"));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomer(
        Guid id,
        [FromBody] UpdateCustomerRequest request,
        [FromServices] UpdateCustomerService service,
        [FromServices] IValidator<UpdateCustomerRequest> validator,
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
            ? Ok(ApiResponse<CustomerResponse>.Ok(response, "Customer updated successfully"))
            : NotFound(ApiResponse.Fail("Customer not found"));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(
        Guid id,
        [FromServices] DeleteCustomerService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(id, cancellationToken);

        return result
            ? Ok(ApiResponse.Ok("Customer deleted successfully"))
            : NotFound(ApiResponse.Fail("Customer not found"));
    }
}
