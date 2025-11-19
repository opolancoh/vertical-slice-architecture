using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Common;
using VerticalSliceApp.Api.Features.Shipments;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Customers;

// Response DTO
public record GetCustomerResponse(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    AddressDto Address,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// Service
public class GetCustomerService
{
    private readonly ApplicationDbContext _context;

    public GetCustomerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetCustomerResponse?> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new GetCustomerResponse(
                c.Id,
                c.Name,
                c.Email,
                c.Phone,
                new AddressDto(
                    c.Address.Street,
                    c.Address.City,
                    c.Address.State,
                    c.Address.ZipCode,
                    c.Address.Country),
                c.CreatedAt,
                c.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return customer;
    }
}

// Endpoint
public class GetCustomerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/customers/{id:guid}", HandleAsync)
            .WithName("GetCustomer")
            .WithTags("Customers")
            .Produces<GetCustomerResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        GetCustomerService service,
        CancellationToken cancellationToken)
    {
        var customer = await service.ExecuteAsync(id, cancellationToken);

        return customer is not null
            ? Results.Ok(customer)
            : Results.NotFound();
    }
}
