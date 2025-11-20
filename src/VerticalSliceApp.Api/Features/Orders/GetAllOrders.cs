using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Orders;

// Response DTO
public record GetAllOrdersResponse(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt);

// Service
public class GetAllOrdersService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAllOrdersService> _logger;

    public GetAllOrdersService(
        ApplicationDbContext context,
        ILogger<GetAllOrdersService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<GetAllOrdersResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all orders");

        var orders = await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new GetAllOrdersResponse(
                o.Id,
                o.OrderNumber,
                o.CustomerId,
                o.Status.ToString(),
                o.TotalAmount,
                o.CreatedAt))
            .ToListAsync(cancellationToken);

        return orders;
    }
}
