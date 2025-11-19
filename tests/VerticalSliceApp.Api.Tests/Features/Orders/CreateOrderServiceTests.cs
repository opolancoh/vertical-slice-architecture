using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using VerticalSliceApp.Api.Features.Orders;
using VerticalSliceApp.Api.Infrastructure.Persistence;
using Xunit;

namespace VerticalSliceApp.Api.Tests.Features.Orders;

public class CreateOrderServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<CreateOrderService>> _loggerMock;
    private readonly CreateOrderService _service;

    public CreateOrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<CreateOrderService>>();
        _service = new CreateOrderService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ShouldCreateOrder()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            CustomerId: customerId,
            Items: new List<OrderItemDto>
            {
                new OrderItemDto("Laptop", "LAPTOP-001", 1, 999.99m),
                new OrderItemDto("Mouse", "MOUSE-001", 2, 25.50m)
            }
        );

        // Act
        var result = await _service.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customerId);
        result.OrderNumber.Should().StartWith("ORD-");
        result.Status.Should().Be("Pending");
        result.TotalAmount.Should().Be(999.99m + (2 * 25.50m)); // 1050.99
        result.Id.Should().NotBeEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ShouldSaveOrderToDatabase()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            CustomerId: customerId,
            Items: new List<OrderItemDto>
            {
                new OrderItemDto("Product A", "SKU-A", 3, 15.00m)
            }
        );

        // Act
        var result = await _service.ExecuteAsync(request, CancellationToken.None);

        // Assert
        var savedOrder = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == result.Id);

        savedOrder.Should().NotBeNull();
        savedOrder!.CustomerId.Should().Be(customerId);
        savedOrder.Items.Should().HaveCount(1);
        savedOrder.Items.First().ProductName.Should().Be("Product A");
        savedOrder.Items.First().Quantity.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleItems_ShouldCalculateTotalAmountCorrectly()
    {
        // Arrange
        var request = new CreateOrderRequest(
            CustomerId: Guid.NewGuid(),
            Items: new List<OrderItemDto>
            {
                new OrderItemDto("Item 1", "SKU-1", 2, 10.00m),
                new OrderItemDto("Item 2", "SKU-2", 3, 15.00m),
                new OrderItemDto("Item 3", "SKU-3", 1, 5.00m)
            }
        );

        // Act
        var result = await _service.ExecuteAsync(request, CancellationToken.None);

        // Assert
        // Total = (2 * 10) + (3 * 15) + (1 * 5) = 20 + 45 + 5 = 70
        result.TotalAmount.Should().Be(70.00m);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGenerateUniqueOrderNumbers()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request1 = new CreateOrderRequest(
            CustomerId: customerId,
            Items: new List<OrderItemDto> { new OrderItemDto("Product", "SKU", 1, 10m) }
        );
        var request2 = new CreateOrderRequest(
            CustomerId: customerId,
            Items: new List<OrderItemDto> { new OrderItemDto("Product", "SKU", 1, 10m) }
        );

        // Act
        var result1 = await _service.ExecuteAsync(request1, CancellationToken.None);
        var result2 = await _service.ExecuteAsync(request2, CancellationToken.None);

        // Assert
        result1.OrderNumber.Should().NotBe(result2.OrderNumber);
        result1.OrderNumber.Should().StartWith("ORD-");
        result2.OrderNumber.Should().StartWith("ORD-");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogInformation()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            CustomerId: customerId,
            Items: new List<OrderItemDto> { new OrderItemDto("Test", "TEST-001", 1, 1m) }
        );

        // Act
        await _service.ExecuteAsync(request, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created order")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
