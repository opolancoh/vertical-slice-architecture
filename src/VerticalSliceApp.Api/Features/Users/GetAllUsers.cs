using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Users;

// Response DTO
public record GetAllUsersResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateTime CreatedAt);

// Service
public class GetAllUsersService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAllUsersService> _logger;

    public GetAllUsersService(
        ApplicationDbContext context,
        ILogger<GetAllUsersService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<GetAllUsersResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all users");

        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new GetAllUsersResponse(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.PhoneNumber,
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        return users;
    }
}
