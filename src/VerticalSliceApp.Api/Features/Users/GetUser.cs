using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Common;
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

// Endpoint
public class GetUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users/{id:guid}", HandleAsync)
            .WithName("GetUser")
            .WithTags("Users")
            .Produces<GetUserResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        GetUserService service,
        CancellationToken cancellationToken)
    {
        var user = await service.ExecuteAsync(id, cancellationToken);

        return user is not null
            ? Results.Ok(user)
            : Results.NotFound();
    }
}
