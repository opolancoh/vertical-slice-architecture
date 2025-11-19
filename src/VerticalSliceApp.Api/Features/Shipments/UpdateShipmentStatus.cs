using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Domain.Entities;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Shipments;

// Request DTO
public record UpdateShipmentStatusRequest(string Status);

// Validator
public class UpdateShipmentStatusValidator : AbstractValidator<UpdateShipmentStatusRequest>
{
    public UpdateShipmentStatusValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => Enum.TryParse<ShipmentStatus>(status, true, out _))
            .WithMessage("Invalid shipment status");
    }
}

// Service
public class UpdateShipmentStatusService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdateShipmentStatusService> _logger;

    public UpdateShipmentStatusService(
        ApplicationDbContext context,
        ILogger<UpdateShipmentStatusService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ShipmentResponse?> ExecuteAsync(
        Guid id,
        UpdateShipmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (shipment is null)
        {
            return null;
        }

        var newStatus = Enum.Parse<ShipmentStatus>(request.Status, true);

        try
        {
            // Use the domain methods to update status
            switch (newStatus)
            {
                case ShipmentStatus.Processing:
                    shipment.Process();
                    break;
                case ShipmentStatus.Dispatched:
                    shipment.Dispatch();
                    break;
                case ShipmentStatus.InTransit:
                    shipment.Transit();
                    break;
                case ShipmentStatus.Delivered:
                    shipment.Deliver();
                    break;
                case ShipmentStatus.Cancelled:
                    shipment.Cancel();
                    break;
                default:
                    throw new InvalidOperationException($"Cannot update to status {newStatus}");
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated shipment {ShipmentNumber} status to {Status}",
                shipment.Number, newStatus);

            return new ShipmentResponse(
                shipment.Id,
                shipment.Number,
                shipment.OrderId,
                shipment.Carrier,
                shipment.ReceiverEmail,
                shipment.Status.ToString(),
                shipment.CreatedAt);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"Cannot update shipment status: {ex.Message}", ex);
        }
    }
}
