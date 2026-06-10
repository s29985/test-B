using Microsoft.EntityFrameworkCore;
using test_B.Entities;

namespace test_B.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).ValueGeneratedOnAdd();
            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Products
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.StockQuantity).IsRequired();
        });

        // Orders
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.OrderId).ValueGeneratedOnAdd();
            entity.Property(e => e.OrderDate).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10,2)").IsRequired();

            // FK to Users, column named Users_UserId per ERD
            entity.Property(e => e.UsersUserId).HasColumnName("Users_UserId").IsRequired();
            entity.HasOne(e => e.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.UsersUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Order_Items
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("Order_Items");
            entity.HasKey(e => new { e.OrderId, e.ProductId });
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)").IsRequired();

            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payments
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(e => e.PaymentId);
            entity.Property(e => e.PaymentId).ValueGeneratedOnAdd();
            entity.Property(e => e.PaymentMethod).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Amount).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.PaymentStatus).HasMaxLength(100).IsRequired();

            // FK to Orders, column named Orders_OrderId per ERD
            entity.Property(e => e.OrdersOrderId).HasColumnName("Orders_OrderId").IsRequired();
            entity.HasOne(e => e.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(e => e.OrdersOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
