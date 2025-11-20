using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Customers;

// Response DTO
public record GetAllCustomersResponse(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    DateTime CreatedAt);

// Service
public class GetAllCustomersService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAllCustomersService> _logger;

    public GetAllCustomersService(
        ApplicationDbContext context,
        ILogger<GetAllCustomersService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<GetAllCustomersResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all customers");

        var customers = await _context.Customers
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new GetAllCustomersResponse(
                c.Id,
                c.Name,
                c.Email,
                c.Phone,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return customers;
    }
}
