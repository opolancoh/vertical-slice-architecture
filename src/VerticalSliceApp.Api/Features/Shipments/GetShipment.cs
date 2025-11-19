using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Common;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Shipments;

// Response DTO
public record GetShipmentResponse(
    Guid Id,
    string Number,
    Guid OrderId,
    string Carrier,
    string ReceiverEmail,
    string TrackingNumber,
    string Status,
    AddressDto Address,
    List<ShipmentItemResponse> Items,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ShipmentItemResponse(
    Guid Id,
    string ProductName,
    string ProductSku,
    int Quantity);

// Service
public class GetShipmentService
{
    private readonly ApplicationDbContext _context;

    public GetShipmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetShipmentResponse?> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var shipment = await _context.Shipments
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new GetShipmentResponse(
                s.Id,
                s.Number,
                s.OrderId,
                s.Carrier,
                s.ReceiverEmail,
                s.TrackingNumber,
                s.Status.ToString(),
                new AddressDto(
                    s.Address.Street,
                    s.Address.City,
                    s.Address.State,
                    s.Address.ZipCode,
                    s.Address.Country),
                s.Items.Select(i => new ShipmentItemResponse(
                    i.Id,
                    i.ProductName,
                    i.ProductSku,
                    i.Quantity)).ToList(),
                s.CreatedAt,
                s.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return shipment;
    }
}

// Endpoint
public class GetShipmentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/shipments/{id:guid}", HandleAsync)
            .WithName("GetShipment")
            .WithTags("Shipments")
            .Produces<GetShipmentResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        GetShipmentService service,
        CancellationToken cancellationToken)
    {
        var shipment = await service.ExecuteAsync(id, cancellationToken);

        return shipment is not null
            ? Results.Ok(shipment)
            : Results.NotFound();
    }
}
