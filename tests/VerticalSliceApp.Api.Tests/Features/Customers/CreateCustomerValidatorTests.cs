using FluentAssertions;
using VerticalSliceApp.Api.Features.Customers;
using VerticalSliceApp.Api.Features.Shipments;
using Xunit;

namespace VerticalSliceApp.Api.Tests.Features.Customers;

public class CreateCustomerValidatorTests
{
    private readonly CreateCustomerValidator _validator;

    public CreateCustomerValidatorTests()
    {
        _validator = new CreateCustomerValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldPass()
    {
        // Arrange
        var request = new CreateCustomerRequest(
            Name: "John Doe",
            Email: "john.doe@example.com",
            Phone: "+1234567890",
            Address: new AddressDto(
                Street: "123 Main St",
                City: "New York",
                State: "NY",
                ZipCode: "10001",
                Country: "USA"
            )
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyName_ShouldFail(string name)
    {
        // Arrange
        var request = new CreateCustomerRequest(
            Name: name,
            Email: "john.doe@example.com",
            Phone: "+1234567890",
            Address: new AddressDto("123 Main St", "New York", "NY", "10001", "USA")
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid-email")]
    public void Validate_InvalidEmail_ShouldFail(string email)
    {
        // Arrange
        var request = new CreateCustomerRequest(
            Name: "John Doe",
            Email: email,
            Phone: "+1234567890",
            Address: new AddressDto("123 Main St", "New York", "NY", "10001", "USA")
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_EmailTooLong_ShouldFail()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com"; // > 255 chars
        var request = new CreateCustomerRequest(
            Name: "John Doe",
            Email: longEmail,
            Phone: "+1234567890",
            Address: new AddressDto("123 Main St", "New York", "NY", "10001", "USA")
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyPhone_ShouldFail(string phone)
    {
        // Arrange
        var request = new CreateCustomerRequest(
            Name: "John Doe",
            Email: "john.doe@example.com",
            Phone: phone,
            Address: new AddressDto("123 Main St", "New York", "NY", "10001", "USA")
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Phone");
    }

    [Fact]
    public void Validate_NullAddress_ShouldFail()
    {
        // Arrange
        var request = new CreateCustomerRequest(
            Name: "John Doe",
            Email: "john.doe@example.com",
            Phone: "+1234567890",
            Address: null!
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Address");
    }

    [Theory]
    [InlineData("", "New York", "NY", "10001", "USA", "Address.Street")]
    [InlineData("123 Main St", "", "NY", "10001", "USA", "Address.City")]
    [InlineData("123 Main St", "New York", "", "10001", "USA", "Address.State")]
    [InlineData("123 Main St", "New York", "NY", "", "USA", "Address.ZipCode")]
    [InlineData("123 Main St", "New York", "NY", "10001", "", "Address.Country")]
    public void Validate_InvalidAddressField_ShouldFail(
        string street, string city, string state, string zipCode, string country, string expectedErrorProperty)
    {
        // Arrange
        var request = new CreateCustomerRequest(
            Name: "John Doe",
            Email: "john.doe@example.com",
            Phone: "+1234567890",
            Address: new AddressDto(street, city, state, zipCode, country)
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == expectedErrorProperty);
    }
}
