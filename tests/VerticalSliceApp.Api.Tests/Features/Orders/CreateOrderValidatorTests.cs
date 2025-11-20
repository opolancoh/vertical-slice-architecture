using FluentAssertions;
using VerticalSliceApp.Api.Features.Orders;
using Xunit;

namespace VerticalSliceApp.Api.Tests.Features.Orders;

public class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator _validator;

    public CreateOrderValidatorTests()
    {
        _validator = new CreateOrderValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldPass()
    {
        // Arrange
        var request = new CreateOrderRequest(
            CustomerId: Guid.NewGuid(),
            Items: new List<OrderItemDto>
            {
                new OrderItemDto("Product A", "SKU-001", 2, 10.99m),
                new OrderItemDto("Product B", "SKU-002", 1, 25.50m)
            }
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyCustomerId_ShouldFail()
    {
        // Arrange
        var request = new CreateOrderRequest(
            CustomerId: Guid.Empty,
            Items: new List<OrderItemDto>
            {
                new OrderItemDto("Product A", "SKU-001", 1, 10.99m)
            }
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
    }

    [Fact]
    public void Validate_EmptyItems_ShouldFail()
    {
        // Arrange
        var request = new CreateOrderRequest(
            CustomerId: Guid.NewGuid(),
            Items: new List<OrderItemDto>()
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyProductName_ShouldFail(string productName)
    {
        // Arrange
        var request = new CreateOrderRequest(
            CustomerId: Guid.NewGuid(),
            Items: new List<OrderItemDto>
            {
                new OrderItemDto(productName, "SKU-001", 1, 10.99m)
            }
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("ProductName"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyProductSku_ShouldFail(string productSku)
    {
        // Arrange
        var request = new CreateOrderRequest(
            CustomerId: Guid.NewGuid(),
            Items: new List<OrderItemDto>
            {
                new OrderItemDto("Product A", productSku, 1, 10.99m)
            }
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("ProductSku"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidQuantity_ShouldFail(int quantity)
    {
        // Arrange
        var request = new CreateOrderRequest(
            CustomerId: Guid.NewGuid(),
            Items: new List<OrderItemDto>
            {
                new OrderItemDto("Product A", "SKU-001", quantity, 10.99m)
            }
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Quantity"));
    }

    [Fact]
    public void Validate_NegativeUnitPrice_ShouldFail()
    {
        // Arrange
        var request = new CreateOrderRequest(
            CustomerId: Guid.NewGuid(),
            Items: new List<OrderItemDto>
            {
                new OrderItemDto("Product A", "SKU-001", 1, -10.99m)
            }
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("UnitPrice"));
    }

    [Fact]
    public void Validate_ZeroUnitPrice_ShouldPass()
    {
        // Arrange
        var request = new CreateOrderRequest(
            CustomerId: Guid.NewGuid(),
            Items: new List<OrderItemDto>
            {
                new OrderItemDto("Free Product", "SKU-FREE", 1, 0m)
            }
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MultipleInvalidItems_ShouldFailWithMultipleErrors()
    {
        // Arrange
        var request = new CreateOrderRequest(
            CustomerId: Guid.NewGuid(),
            Items: new List<OrderItemDto>
            {
                new OrderItemDto("", "", 0, -1m),
                new OrderItemDto("Valid Product", "SKU-001", 1, 10m)
            }
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
    }
}
