using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Features.Shipments;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Customers;

// Request DTO
public record UpdateCustomerRequest(
    string Name,
    string Email,
    string Phone,
    AddressDto Address);

// Validator
public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Address).NotNull();
        RuleFor(x => x.Address.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address.State).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Address.ZipCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Address.Country).NotEmpty().MaximumLength(100);
    }
}

// Service
public class UpdateCustomerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdateCustomerService> _logger;

    public UpdateCustomerService(
        ApplicationDbContext context,
        ILogger<UpdateCustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CustomerResponse?> ExecuteAsync(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer == null)
        {
            return null;
        }

        // Update customer details
        customer.UpdateDetails(request.Name, request.Email, request.Phone);

        // Update address
        var address = Address.Create(
            request.Address.Street,
            request.Address.City,
            request.Address.State,
            request.Address.ZipCode,
            request.Address.Country);
        customer.UpdateAddress(address);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated customer {CustomerId}", id);

        return new CustomerResponse(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.CreatedAt);
    }
}
