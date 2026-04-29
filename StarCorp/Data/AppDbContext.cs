using Microsoft.EntityFrameworkCore;
using StarCorp.Models;
using System;

namespace StarCorp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Buyer> Buyer { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<LineItem> LineItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var productId1 = Guid.NewGuid();
            var buyerId1 = Guid.NewGuid();
            var orderId1 = Guid.NewGuid();
            var orderLineId1 = Guid.NewGuid();

            var productId2 = Guid.NewGuid();
            var buyerId2 = Guid.NewGuid();
            var orderId2 = Guid.NewGuid();
            var orderLineId2 = Guid.NewGuid();

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = productId1,
                    Name = "Gaming Laptop",
                    Description = "Laptop",
                    Brand = "Lenovo",
                    Price = 1500m,
                    Category = "Electronics",
                    Stock = 10
                },
                new Product
                {
                    Id = productId2,
                    Name = "Wireless Mouse",
                    Description = "Good Wireless mouse",
                    Brand = "Logitech",
                    Price = 500m,
                    Category = "Electronics",
                    Stock = 50
                }
            );

            modelBuilder.Entity<Buyer>().HasData(
                new Buyer
                {
                    Id = buyerId1,
                    Name = "Albin Test",
                    Email = "albin.test@sqli.com",
                    DeliveryAddress = "Göteborgsvägen 123",
                    PostalCode = 43330
                },
                new Buyer
                {
                    Id = buyerId2,
                    Name = "Test Person 2",
                    Email = "Test.test@sql.com",
                    DeliveryAddress = "Kyrkogatan 26",
                    PostalCode = 43331
                }
            );
            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    Id = orderId1,
                    TotalValue = 1500m
                },
                new Order
                {
                    Id = orderId2,
                    TotalValue = 1000m
                }
            );

            modelBuilder.Entity<LineItem>().HasData(
                new
                {
                    Id = orderLineId1,
                    ProductId = productId1,
                    Quantity = 1u,
                    Price = 1500m,
                    OrderId = orderId1
                },
                new
                {
                    Id = orderLineId2,
                    ProductId = productId2,
                    Quantity = 2u,
                    Price = 500m,
                    OrderId = orderId2
                }
            );
        }
    }
}