using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using VerticalSliceApp.Api.Features.Customers;
using VerticalSliceApp.Api.Features.Orders;
using VerticalSliceApp.Api.Features.Shipments;
using Xunit;

namespace VerticalSliceApp.Api.Tests.Integration;

public class OrderControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrderControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreatedOrder()
    {
        // Arrange - First create a customer
        var customerRequest = new CreateCustomerRequest(
            Name: "Order Test Customer",
            Email: "ordercustomer@test.com",
            Phone: "+1111111111",
            Address: new AddressDto("123 St", "City", "ST", "12345", "Country")
        );
        var customerResponse = await _client.PostAsJsonAsync("/api/customers", customerRequest);
        var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        var orderRequest = new CreateOrderRequest(
            CustomerId: customer!.Id,
            Items: new List<OrderItemDto>
            {
                new OrderItemDto("Laptop", "LAPTOP-001", 1, 1299.99m),
                new OrderItemDto("Mouse", "MOUSE-001", 2, 25.99m)
            }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", orderRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order.Should().NotBeNull();
        order!.CustomerId.Should().Be(customer.Id);
        order.OrderNumber.Should().StartWith("ORD-");
        order.Status.Should().Be("Pending");
        order.TotalAmount.Should().Be(1299.99m + (2 * 25.99m));
    }

    [Fact]
    public async Task CreateOrder_InvalidRequest_EmptyItems_ReturnsBadRequest()
    {
        // Arrange
        var orderRequest = new CreateOrderRequest(
            CustomerId: Guid.NewGuid(),
            Items: new List<OrderItemDto>()
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", orderRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_InvalidQuantity_ReturnsBadRequest()
    {
        // Arrange
        var orderRequest = new CreateOrderRequest(
            CustomerId: Guid.NewGuid(),
            Items: new List<OrderItemDto>
            {
                new OrderItemDto("Product", "SKU-001", -1, 10.00m)
            }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", orderRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrder_ExistingOrder_ReturnsOrder()
    {
        // Arrange - Create customer and order
        var customerRequest = new CreateCustomerRequest(
            "Test Customer", "customer@test.com", "+1234567890",
            new AddressDto("123 St", "City", "ST", "12345", "Country")
        );
        var customerResponse = await _client.PostAsJsonAsync("/api/customers", customerRequest);
        var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        var orderRequest = new CreateOrderRequest(
            CustomerId: customer!.Id,
            Items: new List<OrderItemDto>
            {
                new OrderItemDto("Product", "SKU-001", 1, 50.00m)
            }
        );
        var createOrderResponse = await _client.PostAsJsonAsync("/api/orders", orderRequest);
        var createdOrder = await createOrderResponse.Content.ReadFromJsonAsync<OrderResponse>();

        // Act
        var response = await _client.GetAsync($"/api/orders/{createdOrder!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order.Should().NotBeNull();
        order!.Id.Should().Be(createdOrder.Id);
        order.OrderNumber.Should().Be(createdOrder.OrderNumber);
    }

    [Fact]
    public async Task GetOrder_NonExistingOrder_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/orders/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
