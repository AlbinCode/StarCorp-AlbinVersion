using FakeItEasy;
using Microsoft.AspNetCore.Http.HttpResults;
using StarCorp.Abstractions;
using StarCorp.Data;
using StarCorp.Endpoints;
using StarCorp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace StarCorp.Tests.Endpoints
{
    public class ProductEndpointsTests
    {
        private readonly IProductDataService _productDataService;

        public ProductEndpointsTests()
        {
            _productDataService = A.Fake<IProductDataService>();
        }

        [Fact]
        public async Task GetProducts_ShouldReturnOk_WithListOfProducts()
        {
            var fakeProducts = new List<IProduct>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Gaming Laptop",
                    Description = "Laptop",
                    Brand = "Lenovo",
                    Price = 1500m,
                    Category = "Electronics",
                    Stock = 10
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Wireless Mouse",
                    Description = "Good Wireless mouse",
                    Brand = "Logitech",
                    Price = 500m,
                    Category = "Electronics",
                    Stock = 50
                }
            };

            A.CallTo(() => _productDataService.GetProductsAsync())
                .Returns(Task.FromResult(fakeProducts.AsQueryable()));

            var result = await ProductEndpoints.GetProducts(null, null, null, _productDataService);

            var okResult = Assert.IsType<Ok<List<IProduct>>>(result);
            Assert.Equal(2, okResult.Value.Count);
        }

        [Fact]
        public async Task CreateProduct_ShouldReturnOk_WhenSuccessful()
        {
            var newProduct = new Product { Id = Guid.NewGuid(), Name = "Monitor", Price = 1000m };

            A.CallTo(() => _productDataService.CreateProductAsync(newProduct))
                .Returns(Task.FromResult<IProduct>(newProduct));

            var result = await ProductEndpoints.CreateProduct(newProduct, _productDataService);

            var okResult = Assert.IsType<Ok<Product>>(result);
            Assert.Equal("Keyboard", okResult.Value.Name);

            A.CallTo(() => _productDataService.CreateProductAsync(newProduct))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnOk_WhenSuccessful()
        {
            var productId = Guid.NewGuid();
            var updatedProduct = new Product { Id = productId, Name = "Updated Keyboard" };

            A.CallTo(() => _productDataService.UpdateProductAsync(updatedProduct))
                .Returns(Task.FromResult<IProduct>(updatedProduct));

            var result = await ProductEndpoints.UpdateProduct(productId, updatedProduct, _productDataService);

            var okResult = Assert.IsType<Ok<Product>>(result);
            Assert.Equal(productId, okResult.Value.Id);

            A.CallTo(() => _productDataService.UpdateProductAsync(updatedProduct))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task DeleteProduct_ShouldReturnOk_WhenSuccessful()
        {
            var productId = Guid.NewGuid();
            var productToDelete = new Product { Id = productId };
            var fakeProducts = new List<IProduct> { productToDelete };

            A.CallTo(() => _productDataService.GetProductsAsync())
                .Returns(Task.FromResult(fakeProducts.AsQueryable()));

            var result = await ProductEndpoints.DeleteProduct(productId, _productDataService);

            Assert.IsType<Ok>(result);

            A.CallTo(() => _productDataService.DeleteProductAsync(productToDelete))
                .MustHaveHappenedOnceExactly();
        }
    }
}