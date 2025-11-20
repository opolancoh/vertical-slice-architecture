using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Customers;

// Service
public class DeleteCustomerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeleteCustomerService> _logger;

    public DeleteCustomerService(
        ApplicationDbContext context,
        ILogger<DeleteCustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer == null)
        {
            return false;
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted customer {CustomerId}", id);

        return true;
    }
}
