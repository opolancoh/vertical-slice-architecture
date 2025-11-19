using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using VerticalSliceApp.Api.Common.Extensions;
using VerticalSliceApp.Api.Features.Customers;

namespace VerticalSliceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        [FromServices] CreateCustomerService service,
        [FromServices] IValidator<CreateCustomerRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(validationResult.ToDictionary());
        }

        var response = await service.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCustomer), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetCustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomer(
        Guid id,
        [FromServices] GetCustomerService service,
        CancellationToken cancellationToken)
    {
        var customer = await service.ExecuteAsync(id, cancellationToken);

        return customer is not null
            ? Ok(customer)
            : NotFound();
    }
}
