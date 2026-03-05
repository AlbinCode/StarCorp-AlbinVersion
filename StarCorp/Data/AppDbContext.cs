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

            var productId1 = Guid.Parse("c27f0e32-b1a7-4b93-bd62-0b91e9d8d711");
            var productId2 = Guid.Parse("a951a77f-1055-4d3a-9d1c-965a2606f121");
            var orderId = Guid.Parse("10000000-0000-0000-0000-000000000021");
            var orderLineId = Guid.Parse("aaaa0025-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var orderId2 = Guid.Parse("10000000-0000-0000-0000-000000000020");
            var orderLineId2 = Guid.Parse("aaaa0026-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

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
                    Id = orderId,
                    Buyer = "Albin Test",
                    BuyerEmail = "albin.test@test.com",
                    DeliveryAddress = "Göteborgsvägen 123",
                    TotalValue = 15000
                }
            );

            modelBuilder.Entity<OrderLine>().HasData(
                new OrderLine
                {
                    Id = orderLineId,
                    OrderId = orderId,
                    ProductId = productId1,
                    Quantity = 1,
                }
            );

            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    Id = orderId2,
                    Buyer = "Test Person 2",
                    BuyerEmail = "Test.test@test.com",
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