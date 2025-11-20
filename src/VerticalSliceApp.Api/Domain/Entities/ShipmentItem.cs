namespace VerticalSliceApp.Api.Domain.Entities;

public class ShipmentItem
{
    public Guid Id { get; private set; }
    public Guid ShipmentId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string ProductSku { get; private set; } = string.Empty;
    public int Quantity { get; private set; }

    private ShipmentItem() { }

    public static ShipmentItem Create(string productName, string productSku, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        return new ShipmentItem
        {
            Id = Guid.NewGuid(),
            ProductName = productName,
            ProductSku = productSku,
            Quantity = quantity
        };
    }
}
