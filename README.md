# Vertical Slice Architecture API

A .NET 8 Web API demonstrating **Vertical Slice Architecture** with EF Core and PostgreSQL.

## Architecture Overview

This project follows the Vertical Slice Architecture pattern as described in the article. Instead of organizing code by technical layers (Controllers, Services, Repositories), it's organized by features (vertical slices).

### Key Principles

- **No CQRS pattern** - Simple service-based approach
- **Feature-focused organization** - Each feature contains everything it needs
- **Rich Domain Model** - Domain entities encapsulate business logic
- **EF Core Direct Access** - No repository pattern abstraction
- **Minimal coupling** - Features are independent of each other

## Project Structure

```
src/VerticalSliceApp.Api/
├── Domain/
│   ├── Entities/           # Rich domain entities
│   │   ├── User.cs
│   │   ├── Customer.cs
│   │   ├── Order.cs
│   │   ├── OrderItem.cs
│   │   ├── Shipment.cs
│   │   └── ShipmentItem.cs
│   └── ValueObjects/
│       └── Address.cs
│
├── Features/               # Vertical slices organized by feature
│   ├── Users/
│   │   ├── CreateUser.cs  # Contains: Request, Response, Validator, Service, Endpoint
│   │   └── GetUser.cs
│   ├── Customers/
│   │   ├── CreateCustomer.cs
│   │   └── GetCustomer.cs
│   ├── Orders/
│   │   ├── CreateOrder.cs
│   │   └── GetOrder.cs
│   └── Shipments/
│       ├── CreateShipment.cs
│       ├── GetShipment.cs
│       └── UpdateShipmentStatus.cs
│
├── Infrastructure/
│   └── Persistence/
│       └── ApplicationDbContext.cs  # EF Core DbContext
│
├── Common/
│   └── IEndpoint.cs       # Common interface for endpoints
│
├── Program.cs             # Application setup and DI configuration
└── appsettings.json       # Configuration including DB connection string
```

## Domain Entities

### User
- Represents system users
- Properties: Email, FirstName, LastName, PhoneNumber

### Customer
- Represents customers who place orders
- Properties: Name, Email, Phone, Address
- Has owned Address value object

### Order
- Represents customer orders
- Properties: OrderNumber, CustomerId, Status, TotalAmount
- Contains OrderItems collection
- Business rules for order lifecycle (Pending → Confirmed → Processing → Shipped → Delivered)

### Shipment
- Represents shipments for orders
- Properties: Number, OrderId, Carrier, ReceiverEmail, TrackingNumber, Status, Address
- Contains ShipmentItems collection
- Business rules for shipment lifecycle (Created → Processing → Dispatched → InTransit → Delivered)

## Feature Slice Structure

Each feature file contains:

1. **Request DTOs** - Input models for the operation
2. **Response DTOs** - Output models for the operation
3. **Validators** - FluentValidation rules
4. **Service** - Business logic implementation
5. **Endpoint** - Minimal API endpoint definition

Example: `CreateShipment.cs` contains everything needed for creating a shipment.

## Prerequisites

- .NET 8 SDK
- PostgreSQL database

## Setup

1. **Update Connection String**

   Edit `appsettings.json` and update the PostgreSQL connection string:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=vertical_slice_db;Username=your_user;Password=your_password"
   }
   ```

2. **Install EF Core Tools** (if not already installed)

   ```bash
   dotnet tool install --global dotnet-ef
   ```

3. **Create Initial Migration**

   ```bash
   cd src/VerticalSliceApp.Api
   dotnet ef migrations add InitialCreate
   ```

4. **Create/Update Database**

   ```bash
   dotnet ef database update
   ```

5. **Run the Application**

   ```bash
   dotnet run
   ```

   The API will be available at `https://localhost:5001` (or the port shown in console)

## API Endpoints

### Users

- `POST /api/users` - Create a new user
- `GET /api/users/{id}` - Get user by ID

### Customers

- `POST /api/customers` - Create a new customer
- `GET /api/customers/{id}` - Get customer by ID

### Orders

- `POST /api/orders` - Create a new order
- `GET /api/orders/{id}` - Get order by ID

### Shipments

- `POST /api/shipments` - Create a new shipment
- `GET /api/shipments/{id}` - Get shipment by ID
- `PATCH /api/shipments/{id}/status` - Update shipment status

## Example Requests

### Create a Customer

```json
POST /api/customers
{
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "+1234567890",
  "address": {
    "street": "123 Main St",
    "city": "New York",
    "state": "NY",
    "zipCode": "10001",
    "country": "USA"
  }
}
```

### Create an Order

```json
POST /api/orders
{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "items": [
    {
      "productName": "Laptop",
      "productSku": "LAP-001",
      "quantity": 2,
      "unitPrice": 999.99
    }
  ]
}
```

### Create a Shipment

```json
POST /api/shipments
{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "carrier": "FedEx",
  "receiverEmail": "customer@example.com",
  "address": {
    "street": "456 Oak Ave",
    "city": "Los Angeles",
    "state": "CA",
    "zipCode": "90001",
    "country": "USA"
  },
  "items": [
    {
      "productName": "Laptop",
      "productSku": "LAP-001",
      "quantity": 2
    }
  ]
}
```

### Update Shipment Status

```json
PATCH /api/shipments/{id}/status
{
  "status": "Processing"
}
```

Valid statuses: `Created`, `Processing`, `Dispatched`, `InTransit`, `Delivered`, `Cancelled`

## Architecture Benefits

### 1. Feature Cohesion
All code related to a specific feature is in one place, making it easy to understand and modify.

### 2. Reduced Coupling
Features are independent - changes to one feature don't affect others.

### 3. Fast Development
No need to navigate between multiple projects/folders - everything for a feature is in one file.

### 4. Easy Testing
Each feature can be tested independently with all dependencies visible.

### 5. Rich Domain Model
Business logic is encapsulated in domain entities, not scattered across services.

### 6. Scalable Teams
Different developers/teams can work on different features without conflicts.

## Technologies Used

- **.NET 8** - Latest .NET framework
- **ASP.NET Core Minimal APIs** - Lightweight API endpoints
- **Entity Framework Core 8** - ORM for database access
- **PostgreSQL** - Relational database
- **Npgsql** - PostgreSQL provider for EF Core
- **FluentValidation** - Input validation
- **Swagger/OpenAPI** - API documentation

## Development Notes

### Adding a New Feature

1. Create a new file in the appropriate `Features/{Entity}` folder
2. Define Request/Response DTOs
3. Create a Validator using FluentValidation
4. Implement the Service with business logic
5. Create the Endpoint implementing `IEndpoint`
6. The endpoint and service will be auto-registered via reflection in `Program.cs`

### Database Migrations

```bash
# Add a new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove
```

## License

This is a sample project for educational purposes.
