using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using VerticalSliceApp.Api.Common;
using VerticalSliceApp.Api.Common.Extensions;
using VerticalSliceApp.Api.Features.Orders;

namespace VerticalSliceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<GetAllOrdersResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrders(
        [FromServices] GetAllOrdersService service,
        CancellationToken cancellationToken)
    {
        var orders = await service.ExecuteAsync(cancellationToken);
        return Ok(ApiResponse<List<GetAllOrdersResponse>>.Ok(orders));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(
        Guid id,
        [FromServices] GetOrderService service,
        CancellationToken cancellationToken)
    {
        var order = await service.ExecuteAsync(id, cancellationToken);

        return order is not null
            ? Ok(ApiResponse<GetOrderResponse>.Ok(order))
            : NotFound(ApiResponse.Fail("Order not found"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        [FromServices] CreateOrderService service,
        [FromServices] IValidator<CreateOrderRequest> validator,
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
            nameof(GetOrder),
            new { id = response.Id },
            ApiResponse<OrderResponse>.Ok(response, "Order created successfully"));
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        [FromServices] UpdateOrderStatusService service,
        [FromServices] IValidator<UpdateOrderStatusRequest> validator,
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
                ? Ok(ApiResponse<OrderResponse>.Ok(response, "Order status updated successfully"))
                : NotFound(ApiResponse.Fail("Order not found"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(
        Guid id,
        [FromServices] DeleteOrderService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(id, cancellationToken);

        return result
            ? Ok(ApiResponse.Ok("Order deleted successfully"))
            : NotFound(ApiResponse.Fail("Order not found"));
    }
}
