using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Users;

// Request DTO
public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string PhoneNumber);

// Validator
public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
    }
}

// Service
public class UpdateUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdateUserService> _logger;

    public UpdateUserService(
        ApplicationDbContext context,
        ILogger<UpdateUserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserResponse?> ExecuteAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            return null;
        }

        user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated user {UserId}", id);

        return new UserResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.CreatedAt);
    }
}
