using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using VerticalSliceApp.Api.Common.Extensions;
using VerticalSliceApp.Api.Features.Shipments;

namespace VerticalSliceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateShipment(
        [FromBody] CreateShipmentRequest request,
        [FromServices] CreateShipmentService service,
        [FromServices] IValidator<CreateShipmentRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var response = await service.ExecuteAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetShipment), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShipment(
        Guid id,
        [FromServices] GetShipmentService service,
        CancellationToken cancellationToken)
    {
        var shipment = await service.ExecuteAsync(id, cancellationToken);

        return shipment is not null
            ? Ok(shipment)
            : NotFound();
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateShipmentStatus(
        Guid id,
        [FromBody] UpdateShipmentStatusRequest request,
        [FromServices] UpdateShipmentStatusService service,
        [FromServices] IValidator<UpdateShipmentStatusRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var response = await service.ExecuteAsync(id, request, cancellationToken);

            return response is not null
                ? Ok(response)
                : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
