using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Shipments;

// Response DTO
public record GetAllShipmentsResponse(
    Guid Id,
    string Number,
    Guid OrderId,
    string Carrier,
    string ReceiverEmail,
    string Status,
    DateTime CreatedAt);

// Service
public class GetAllShipmentsService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAllShipmentsService> _logger;

    public GetAllShipmentsService(
        ApplicationDbContext context,
        ILogger<GetAllShipmentsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<GetAllShipmentsResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all shipments");

        var shipments = await _context.Shipments
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new GetAllShipmentsResponse(
                s.Id,
                s.Number,
                s.OrderId,
                s.Carrier,
                s.ReceiverEmail,
                s.Status.ToString(),
                s.CreatedAt))
            .ToListAsync(cancellationToken);

        return shipments;
    }
}
