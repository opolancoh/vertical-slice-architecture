using FluentValidation;
using VerticalSliceApp.Api.Common;
using VerticalSliceApp.Api.Domain.Entities;
using VerticalSliceApp.Api.Features.Shipments;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Customers;

// Request and Response DTOs
public record CreateCustomerRequest(
    string Name,
    string Email,
    string Phone,
    AddressDto Address);

public record CustomerResponse(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    DateTime CreatedAt);

// Validator
public class CreateCustomerValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerValidator()
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
public class CreateCustomerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateCustomerService> _logger;

    public CreateCustomerService(
        ApplicationDbContext context,
        ILogger<CreateCustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CustomerResponse> ExecuteAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        // Create address
        var address = Address.Create(
            request.Address.Street,
            request.Address.City,
            request.Address.State,
            request.Address.ZipCode,
            request.Address.Country);

        // Create customer
        var customer = Customer.Create(
            request.Name,
            request.Email,
            request.Phone,
            address);

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created customer {CustomerEmail}", request.Email);

        return new CustomerResponse(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.CreatedAt);
    }
}

// Endpoint
public class CreateCustomerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/customers", HandleAsync)
            .WithName("CreateCustomer")
            .WithTags("Customers")
            .Produces<CustomerResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> HandleAsync(
        CreateCustomerRequest request,
        CreateCustomerService service,
        IValidator<CreateCustomerRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var response = await service.ExecuteAsync(request, cancellationToken);
        return Results.Created($"/api/customers/{response.Id}", response);
    }
}
