namespace VerticalSliceApp.Api.Domain.Entities;

public enum ShipmentStatus
{
    Created,
    Processing,
    Dispatched,
    InTransit,
    Delivered,
    Cancelled
}

public class Shipment
{
    private readonly List<ShipmentItem> _items = new();

    public Guid Id { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public Guid OrderId { get; private set; }
    public Address Address { get; private set; } = null!;
    public string Carrier { get; private set; } = string.Empty;
    public string ReceiverEmail { get; private set; } = string.Empty;
    public string TrackingNumber { get; private set; } = string.Empty;
    public ShipmentStatus Status { get; private set; }
    public IReadOnlyList<ShipmentItem> Items => _items.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Shipment() { }

    public static Shipment Create(
        string number,
        Guid orderId,
        Address address,
        string carrier,
        string receiverEmail,
        List<ShipmentItem> items)
    {
        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            Number = number,
            OrderId = orderId,
            Address = address,
            Carrier = carrier,
            ReceiverEmail = receiverEmail,
            Status = ShipmentStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        shipment.AddItems(items);

        return shipment;
    }

    public void AddItem(ShipmentItem item)
    {
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddItems(List<ShipmentItem> items)
    {
        _items.AddRange(items);
    }

    public void RemoveItem(ShipmentItem item)
    {
        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAddress(Address newAddress)
    {
        if (Status != ShipmentStatus.Created)
            throw new InvalidOperationException("Can only update address for created shipments");

        Address = newAddress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignTrackingNumber(string trackingNumber)
    {
        TrackingNumber = trackingNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Process()
    {
        if (Status != ShipmentStatus.Created)
            throw new InvalidOperationException("Can only process created shipments");

        Status = ShipmentStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Dispatch()
    {
        if (Status != ShipmentStatus.Processing)
            throw new InvalidOperationException("Can only dispatch processing shipments");

        Status = ShipmentStatus.Dispatched;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Transit()
    {
        if (Status != ShipmentStatus.Dispatched)
            throw new InvalidOperationException("Can only mark as in transit after dispatch");

        Status = ShipmentStatus.InTransit;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deliver()
    {
        if (Status != ShipmentStatus.InTransit)
            throw new InvalidOperationException("Can only deliver shipments in transit");

        Status = ShipmentStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ShipmentStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel delivered shipments");

        Status = ShipmentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
