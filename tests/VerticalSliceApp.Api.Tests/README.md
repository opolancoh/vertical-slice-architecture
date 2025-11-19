# VerticalSliceApp.Api.Tests

This project contains comprehensive tests for the Vertical Slice Architecture API.

## Test Structure

```
VerticalSliceApp.Api.Tests/
├── Features/
│   ├── Customers/
│   │   ├── CreateCustomerValidatorTests.cs    # Validator unit tests
│   │   └── CreateCustomerServiceTests.cs      # Service unit tests
│   └── Orders/
│       ├── CreateOrderValidatorTests.cs       # Validator unit tests
│       └── CreateOrderServiceTests.cs         # Service unit tests
└── Integration/
    ├── CustomWebApplicationFactory.cs         # Test factory for integration tests
    ├── CustomerControllerIntegrationTests.cs  # Customer API integration tests
    └── OrderControllerIntegrationTests.cs     # Order API integration tests
```

## Test Categories

### 1. Validator Tests
Tests for FluentValidation validators to ensure input validation rules work correctly.
- Empty/null value validation
- Format validation (email, etc.)
- Length constraints
- Business rule validation

### 2. Service Unit Tests
Tests for service classes using in-memory database.
- Business logic validation
- Data persistence
- Logging verification
- Edge cases

### 3. Integration Tests
End-to-end API tests using `WebApplicationFactory`.
- HTTP request/response validation
- Status code verification
- Full request pipeline testing
- Database operations

## Dependencies

- **xUnit**: Test framework
- **FluentAssertions**: Fluent assertion library
- **Moq**: Mocking framework
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing support
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing

## Running the Tests

### Run all tests
```bash
dotnet test
```

### Run tests from specific project
```bash
dotnet test tests/VerticalSliceApp.Api.Tests/VerticalSliceApp.Api.Tests.csproj
```

### Run tests with verbosity
```bash
dotnet test --verbosity detailed
```

### Run tests with coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run specific test class
```bash
dotnet test --filter "FullyQualifiedName~CreateCustomerValidatorTests"
```

### Run specific test method
```bash
dotnet test --filter "FullyQualifiedName~CreateCustomerValidatorTests.Validate_ValidRequest_ShouldPass"
```

## Test Patterns

### Arrange-Act-Assert (AAA)
All tests follow the AAA pattern:
```csharp
[Fact]
public async Task TestMethod()
{
    // Arrange - Set up test data and dependencies
    var request = new CreateCustomerRequest(...);

    // Act - Execute the method under test
    var result = await _service.ExecuteAsync(request);

    // Assert - Verify the results
    result.Should().NotBeNull();
}
```

### Test Naming Convention
Tests use descriptive names: `MethodName_Scenario_ExpectedBehavior`
- `Validate_EmptyName_ShouldFail`
- `ExecuteAsync_ValidRequest_ShouldCreateCustomer`
- `CreateCustomer_NonExistingCustomer_ReturnsNotFound`

## Best Practices

1. **Isolation**: Each test is independent and doesn't rely on other tests
2. **In-Memory Database**: Tests use in-memory databases with unique names to avoid conflicts
3. **Cleanup**: Tests implement `IDisposable` to clean up resources
4. **Realistic Data**: Test data represents realistic scenarios
5. **Clear Assertions**: Use FluentAssertions for readable test assertions

## Extending Tests

When adding new features:
1. Create validator tests in `Features/{FeatureName}/{ValidatorName}Tests.cs`
2. Create service tests in `Features/{FeatureName}/{ServiceName}Tests.cs`
3. Add integration tests in `Integration/{ControllerName}IntegrationTests.cs`
4. Follow existing patterns and naming conventions
