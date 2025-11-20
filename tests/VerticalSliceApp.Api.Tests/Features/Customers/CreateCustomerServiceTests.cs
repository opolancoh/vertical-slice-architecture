using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using VerticalSliceApp.Api.Features.Customers;
using VerticalSliceApp.Api.Features.Shipments;
using VerticalSliceApp.Api.Infrastructure.Persistence;
using Xunit;

namespace VerticalSliceApp.Api.Tests.Features.Customers;

public class CreateCustomerServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<CreateCustomerService>> _loggerMock;
    private readonly CreateCustomerService _service;

    public CreateCustomerServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<CreateCustomerService>>();
        _service = new CreateCustomerService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ShouldCreateCustomer()
    {
        // Arrange
        var request = new CreateCustomerRequest(
            Name: "Jane Smith",
            Email: "jane.smith@example.com",
            Phone: "+1987654321",
            Address: new AddressDto(
                Street: "456 Oak Ave",
                City: "Los Angeles",
                State: "CA",
                ZipCode: "90001",
                Country: "USA"
            )
        );

        // Act
        var result = await _service.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Email.Should().Be(request.Email);
        result.Phone.Should().Be(request.Phone);
        result.Id.Should().NotBeEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify customer was saved to database
        var savedCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == result.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.Name.Should().Be(request.Name);
        savedCustomer.Email.Should().Be(request.Email);
        savedCustomer.Address.Street.Should().Be(request.Address.Street);
        savedCustomer.Address.City.Should().Be(request.Address.City);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ShouldLogInformation()
    {
        // Arrange
        var request = new CreateCustomerRequest(
            Name: "Test User",
            Email: "test@example.com",
            Phone: "+1111111111",
            Address: new AddressDto("123 Test St", "Test City", "TC", "12345", "Test Country")
        );

        // Act
        await _service.ExecuteAsync(request, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created customer")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleCustomers_ShouldCreateAll()
    {
        // Arrange
        var request1 = new CreateCustomerRequest(
            "Customer 1", "customer1@example.com", "+1111111111",
            new AddressDto("Address 1", "City 1", "ST", "11111", "Country 1")
        );
        var request2 = new CreateCustomerRequest(
            "Customer 2", "customer2@example.com", "+2222222222",
            new AddressDto("Address 2", "City 2", "ST", "22222", "Country 2")
        );

        // Act
        var result1 = await _service.ExecuteAsync(request1, CancellationToken.None);
        var result2 = await _service.ExecuteAsync(request2, CancellationToken.None);

        // Assert
        result1.Id.Should().NotBe(result2.Id);
        var customers = await _context.Customers.ToListAsync();
        customers.Should().HaveCount(2);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
