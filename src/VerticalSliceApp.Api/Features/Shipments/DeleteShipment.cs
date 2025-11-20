using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Shipments;

// Service
public class DeleteShipmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeleteShipmentService> _logger;

    public DeleteShipmentService(
        ApplicationDbContext context,
        ILogger<DeleteShipmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (shipment == null)
        {
            return false;
        }

        _context.Shipments.Remove(shipment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted shipment {ShipmentId}", id);

        return true;
    }
}
