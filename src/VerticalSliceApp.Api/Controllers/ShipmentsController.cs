using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using VerticalSliceApp.Api.Common;
using VerticalSliceApp.Api.Common.Extensions;
using VerticalSliceApp.Api.Features.Shipments;

namespace VerticalSliceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<GetAllShipmentsResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllShipments(
        [FromServices] GetAllShipmentsService service,
        CancellationToken cancellationToken)
    {
        var shipments = await service.ExecuteAsync(cancellationToken);
        return Ok(ApiResponse<List<GetAllShipmentsResponse>>.Ok(shipments));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetShipmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShipment(
        Guid id,
        [FromServices] GetShipmentService service,
        CancellationToken cancellationToken)
    {
        var shipment = await service.ExecuteAsync(id, cancellationToken);

        return shipment is not null
            ? Ok(ApiResponse<GetShipmentResponse>.Ok(shipment))
            : NotFound(ApiResponse.Fail("Shipment not found"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ShipmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateShipment(
        [FromBody] CreateShipmentRequest request,
        [FromServices] CreateShipmentService service,
        [FromServices] IValidator<CreateShipmentRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse.Fail("Validation failed", errors));
        }

        try
        {
            var response = await service.ExecuteAsync(request, cancellationToken);
            return CreatedAtAction(
                nameof(GetShipment),
                new { id = response.Id },
                ApiResponse<ShipmentResponse>.Ok(response, "Shipment created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
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
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse.Fail("Validation failed", errors));
        }

        try
        {
            var response = await service.ExecuteAsync(id, request, cancellationToken);

            return response is not null
                ? Ok(ApiResponse<ShipmentResponse>.Ok(response, "Shipment status updated successfully"))
                : NotFound(ApiResponse.Fail("Shipment not found"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteShipment(
        Guid id,
        [FromServices] DeleteShipmentService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(id, cancellationToken);

        return result
            ? Ok(ApiResponse.Ok("Shipment deleted successfully"))
            : NotFound(ApiResponse.Fail("Shipment not found"));
    }
}
