using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Users;

// Response DTO
public record GetUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// Service
public class GetUserService
{
    private readonly ApplicationDbContext _context;

    public GetUserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetUserResponse?> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new GetUserResponse(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.PhoneNumber,
                u.CreatedAt,
                u.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }
}
