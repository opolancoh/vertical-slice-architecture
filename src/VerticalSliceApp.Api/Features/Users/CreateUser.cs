using FluentValidation;
using VerticalSliceApp.Api.Common;
using VerticalSliceApp.Api.Domain.Entities;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Users;

// Request and Response DTOs
public record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber);

public record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateTime CreatedAt);

// Validator
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
    }
}

// Service
public class CreateUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateUserService> _logger;

    public CreateUserService(
        ApplicationDbContext context,
        ILogger<CreateUserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserResponse> ExecuteAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        // Create user
        var user = User.Create(
            request.Email,
            request.FirstName,
            request.LastName,
            request.PhoneNumber);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created user {UserEmail}", request.Email);

        return new UserResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.CreatedAt);
    }
}

// Endpoint
public class CreateUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users", HandleAsync)
            .WithName("CreateUser")
            .WithTags("Users")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> HandleAsync(
        CreateUserRequest request,
        CreateUserService service,
        IValidator<CreateUserRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var response = await service.ExecuteAsync(request, cancellationToken);
        return Results.Created($"/api/users/{response.Id}", response);
    }
}
