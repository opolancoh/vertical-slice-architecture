using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Infrastructure.Persistence;

namespace VerticalSliceApp.Api.Features.Users;

// Service
public class DeleteUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeleteUserService> _logger;

    public DeleteUserService(
        ApplicationDbContext context,
        ILogger<DeleteUserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted user {UserId}", id);

        return true;
    }
}
