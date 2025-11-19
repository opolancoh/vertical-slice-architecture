using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using VerticalSliceApp.Api.Features.Customers;
using VerticalSliceApp.Api.Features.Shipments;
using Xunit;

namespace VerticalSliceApp.Api.Tests.Integration;

public class CustomerControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CustomerControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCustomer_ValidRequest_ReturnsCreatedCustomer()
    {
        // Arrange
        var request = new CreateCustomerRequest(
            Name: "Integration Test Customer",
            Email: "integration@test.com",
            Phone: "+1234567890",
            Address: new AddressDto(
                Street: "123 Integration St",
                City: "Test City",
                State: "TS",
                ZipCode: "12345",
                Country: "Test Country"
            )
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customer.Should().NotBeNull();
        customer!.Name.Should().Be(request.Name);
        customer.Email.Should().Be(request.Email);
        customer.Phone.Should().Be(request.Phone);
        customer.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateCustomer_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange - request with empty name
        var request = new CreateCustomerRequest(
            Name: "",
            Email: "test@example.com",
            Phone: "+1234567890",
            Address: new AddressDto("123 St", "City", "ST", "12345", "Country")
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCustomer_ExistingCustomer_ReturnsCustomer()
    {
        // Arrange - First create a customer
        var createRequest = new CreateCustomerRequest(
            Name: "Get Test Customer",
            Email: "gettest@example.com",
            Phone: "+9876543210",
            Address: new AddressDto("456 Get St", "Get City", "GC", "54321", "Get Country")
        );
        var createResponse = await _client.PostAsJsonAsync("/api/customers", createRequest);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        // Act
        var response = await _client.GetAsync($"/api/customers/{createdCustomer!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customer.Should().NotBeNull();
        customer!.Id.Should().Be(createdCustomer.Id);
        customer.Name.Should().Be(createRequest.Name);
    }

    [Fact]
    public async Task GetCustomer_NonExistingCustomer_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/customers/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
