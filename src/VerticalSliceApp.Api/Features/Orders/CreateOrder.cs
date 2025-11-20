using FluentValidation;
using VerticalSliceApp.Api.Domain.Entities;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Orders;

// Request and Response DTOs
public record CreateOrderRequest(
    Guid CustomerId,
    List<OrderItemDto> Items);

public record OrderItemDto(
    string ProductName,
    string ProductSku,
    int Quantity,
    decimal UnitPrice);

public record OrderResponse(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt);

// Validator
public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(200);
            item.RuleFor(i => i.ProductSku).NotEmpty().MaximumLength(50);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

// Service
public class CreateOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateOrderService> _logger;

    public CreateOrderService(
        ApplicationDbContext context,
        ILogger<CreateOrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderResponse> ExecuteAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        // Generate order number
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        // Create order items
        var items = request.Items.Select(item =>
            OrderItem.Create(
                item.ProductName,
                item.ProductSku,
                item.Quantity,
                item.UnitPrice)).ToList();

        // Create order
        var order = Order.Create(
            orderNumber,
            request.CustomerId,
            items);

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created order {OrderNumber} for customer {CustomerId}",
            orderNumber, request.CustomerId);

        return new OrderResponse(
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAt);
    }
}
