using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Common;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Orders;

// Response DTO
public record GetOrderResponse(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    List<GetOrderItemResponse> Items,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record GetOrderItemResponse(
    Guid Id,
    string ProductName,
    string ProductSku,
    int Quantity,
    decimal UnitPrice);

// Service
public class GetOrderService
{
    private readonly ApplicationDbContext _context;

    public GetOrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetOrderResponse?> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => new GetOrderResponse(
                o.Id,
                o.OrderNumber,
                o.CustomerId,
                o.Status.ToString(),
                o.TotalAmount,
                o.Items.Select(i => new GetOrderItemResponse(
                    i.Id,
                    i.ProductName,
                    i.ProductSku,
                    i.Quantity,
                    i.UnitPrice)).ToList(),
                o.CreatedAt,
                o.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return order;
    }
}

// Endpoint
public class GetOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{id:guid}", HandleAsync)
            .WithName("GetOrder")
            .WithTags("Orders")
            .Produces<GetOrderResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        GetOrderService service,
        CancellationToken cancellationToken)
    {
        var order = await service.ExecuteAsync(id, cancellationToken);

        return order is not null
            ? Results.Ok(order)
            : Results.NotFound();
    }
}
