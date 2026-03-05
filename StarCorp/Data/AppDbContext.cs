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
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Product> Products { get; set; }
    

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var orderId1 = Guid.NewGuid();
            var orderLineId1 = Guid.NewGuid();
            var orderId2 = Guid.NewGuid();
            var orderLineId2 = Guid.NewGuid();

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = productId1,
                    Name = "Gaming Laptop",
                    Description = "Laptop",
                    Brand = "Lenovo",
                    Price = 1500,
                    Category = "Electronics",
                    Stock = 10
                },
                new Product
                {
                    Id = productId2,
                    Name = "Wireless Mouse",
                    Description = "Good Wireless mouse",
                    Brand = "Logitech",
                    Price = 500,
                    Category = "Electronics",
                    Stock = 50
                }
            );

            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    Id = orderId1,
                    Buyer = "Albin Test",
                    BuyerEmail = "albin.test@sqli.com",
                    DeliveryAddress = "Göteborgsvägen 123",
                    TotalValue = 15000
                }
            );

            modelBuilder.Entity<OrderLine>().HasData(
                new OrderLine
                {
                    Id = orderLineId1,
                    OrderId = orderId1,
                    ProductId = productId1,
                    Quantity = 1,
                }
            );

            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    Id = orderId2,
                    Buyer = "Test Person 2",
                    BuyerEmail = "Test.test@sqli.com",
                    DeliveryAddress = "Kyrkogatan 26",
                    TotalValue = 1100
                }
            );

            modelBuilder.Entity<OrderLine>().HasData(
                new OrderLine
                {
                    Id = orderLineId2,
                    OrderId = orderId2,
                    ProductId = productId2,
                    Quantity = 2,
                }
            );
        }
    }
}