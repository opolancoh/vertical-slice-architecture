namespace VerticalSliceApp.Api.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Customer() { }

    public static Customer Create(string name, string email, string phone, Address address)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Phone = phone,
            Address = address,
            CreatedAt = DateTime.UtcNow
        };

        return customer;
    }

    public void UpdateDetails(string name, string email, string phone)
    {
        Name = name;
        Email = email;
        Phone = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAddress(Address newAddress)
    {
        Address = newAddress;
        UpdatedAt = DateTime.UtcNow;
    }
}
