using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Orders;

// Service
public class DeleteOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeleteOrderService> _logger;

    public DeleteOrderService(
        ApplicationDbContext context,
        ILogger<DeleteOrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
        {
            return false;
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted order {OrderId}", id);

        return true;
    }
}
