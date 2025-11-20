using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Domain.Entities;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Orders;

// Request DTO
public record UpdateOrderStatusRequest(string Status);

// Validator
public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(BeValidStatus)
            .WithMessage("Status must be one of: Pending, Confirmed, Processing, Shipped, Delivered, Cancelled");
    }

    private bool BeValidStatus(string status)
    {
        return Enum.TryParse<OrderStatus>(status, true, out _);
    }
}

// Service
public class UpdateOrderStatusService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdateOrderStatusService> _logger;

    public UpdateOrderStatusService(
        ApplicationDbContext context,
        ILogger<UpdateOrderStatusService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderResponse?> ExecuteAsync(
        Guid id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
        {
            return null;
        }

        var newStatus = Enum.Parse<OrderStatus>(request.Status, true);

        // Use domain methods to update status based on the target status
        try
        {
            switch (newStatus)
            {
                case OrderStatus.Confirmed:
                    order.Confirm();
                    break;
                case OrderStatus.Processing:
                    order.Process();
                    break;
                case OrderStatus.Shipped:
                    order.Ship();
                    break;
                case OrderStatus.Delivered:
                    order.Deliver();
                    break;
                case OrderStatus.Cancelled:
                    order.Cancel();
                    break;
                default:
                    throw new InvalidOperationException($"Cannot transition to status: {newStatus}");
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated order {OrderId} status to {Status}", id, newStatus);

            return new OrderResponse(
                order.Id,
                order.OrderNumber,
                order.CustomerId,
                order.Status.ToString(),
                order.TotalAmount,
                order.CreatedAt);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update order {OrderId} status: {Error}", id, ex.Message);
            throw;
        }
    }
}
