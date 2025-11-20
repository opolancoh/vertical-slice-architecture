using Microsoft.EntityFrameworkCore;
using VerticalSliceApp.Api.Domain.Entities;

namespace VerticalSliceApp.Api.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.OwnsOne(e => e.Address, address =>
            {
                address.Property(a => a.Street).HasColumnName("address_street").HasMaxLength(200);
                address.Property(a => a.City).HasColumnName("address_city").HasMaxLength(100);
                address.Property(a => a.State).HasColumnName("address_state").HasMaxLength(50);
                address.Property(a => a.ZipCode).HasColumnName("address_zip_code").HasMaxLength(20);
                address.Property(a => a.Country).HasColumnName("address_country").HasMaxLength(100);
            });
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.HasMany<OrderItem>("_items")
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ProductSku).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
        });

        // Shipment configuration
        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.ToTable("shipments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Carrier).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ReceiverEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.TrackingNumber).HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.OwnsOne(e => e.Address, address =>
            {
                address.Property(a => a.Street).HasColumnName("address_street").HasMaxLength(200);
                address.Property(a => a.City).HasColumnName("address_city").HasMaxLength(100);
                address.Property(a => a.State).HasColumnName("address_state").HasMaxLength(50);
                address.Property(a => a.ZipCode).HasColumnName("address_zip_code").HasMaxLength(20);
                address.Property(a => a.Country).HasColumnName("address_country").HasMaxLength(100);
            });
            entity.HasMany<ShipmentItem>("_items")
                .WithOne()
                .HasForeignKey(i => i.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.Number).IsUnique();
            entity.HasIndex(e => e.OrderId);
        });

        // ShipmentItem configuration
        modelBuilder.Entity<ShipmentItem>(entity =>
        {
            entity.ToTable("shipment_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ProductSku).IsRequired().HasMaxLength(50);
        });
    }
}
