using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Domain.Entities;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Shipments;

// Request and Response DTOs
public record CreateShipmentRequest(
    Guid OrderId,
    string Carrier,
    string ReceiverEmail,
    AddressDto Address,
    List<ShipmentItemDto> Items);

public record AddressDto(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country);

public record ShipmentItemDto(
    string ProductName,
    string ProductSku,
    int Quantity);

public record ShipmentResponse(
    Guid Id,
    string Number,
    Guid OrderId,
    string Carrier,
    string ReceiverEmail,
    string Status,
    DateTime CreatedAt);

// Validator
public class CreateShipmentValidator : AbstractValidator<CreateShipmentRequest>
{
    public CreateShipmentValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Carrier).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ReceiverEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Address).NotNull();
        RuleFor(x => x.Address.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address.State).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Address.ZipCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Address.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Items).NotEmpty();
    }
}

// Service
public class CreateShipmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateShipmentService> _logger;

    public CreateShipmentService(
        ApplicationDbContext context,
        ILogger<CreateShipmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ShipmentResponse> ExecuteAsync(
        CreateShipmentRequest request,
        CancellationToken cancellationToken)
    {
        // Check if shipment already exists for this order
        var existingShipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.OrderId == request.OrderId, cancellationToken);

        if (existingShipment != null)
        {
            throw new InvalidOperationException($"Shipment for order '{request.OrderId}' already exists");
        }

        // Generate shipment number
        var shipmentNumber = $"SH-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        // Create address
        var address = Address.Create(
            request.Address.Street,
            request.Address.City,
            request.Address.State,
            request.Address.ZipCode,
            request.Address.Country);

        // Create shipment items
        var items = request.Items.Select(item =>
            ShipmentItem.Create(item.ProductName, item.ProductSku, item.Quantity)).ToList();

        // Create shipment
        var shipment = Shipment.Create(
            shipmentNumber,
            request.OrderId,
            address,
            request.Carrier,
            request.ReceiverEmail,
            items);

        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created shipment {ShipmentNumber} for order {OrderId}",
            shipmentNumber, request.OrderId);

        return new ShipmentResponse(
            shipment.Id,
            shipment.Number,
            shipment.OrderId,
            shipment.Carrier,
            shipment.ReceiverEmail,
            shipment.Status.ToString(),
            shipment.CreatedAt);
    }
}
