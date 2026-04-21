using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using StarCorp.Abstractions;
using StarCorp.Data;
using StarCorp.Exceptions;
using StarCorp.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StarCorp.Endpoints
{
    public static class ProductEndpoints
    {
        public static void MapProductEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/products").WithTags("Products");

            group.MapGet("/", GetProducts);
            group.MapPost("/", CreateProduct);
            group.MapPut("/{id}", UpdateProduct);
            group.MapDelete("/{id}", DeleteProduct);
        }

        public static async Task<IResult> GetProducts(
            [FromQuery] string? query,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            IProductDataService productDataService)
        {
            int currentPage = page ?? 1;
            int currentSize = pageSize ?? 20;

            var allProducts = await productDataService.GetProductsAsync();

            if (!string.IsNullOrEmpty(query))
            {
                string lowerQuery = query.ToLower();

                allProducts = allProducts.Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(lowerQuery)) ||
                    (p.Description != null && p.Description.ToLower().Contains(lowerQuery)) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(lowerQuery)) ||
                    (p.Category != null && p.Category.ToLower().Contains(lowerQuery))
                );
            }

            var result = allProducts
                .Skip((currentPage - 1) * currentSize)
                .Take(currentSize)
                .ToList();

            return Results.Ok(result);
        }

        public static async Task<IResult> CreateProduct(Product product, IProductDataService productDataService)
        {
            if (product.Id == Guid.Empty)
            {
                product.Id = Guid.NewGuid();
            }

            try
            {
                ModelValidationException.ThrowIfInvalid(product);

                await productDataService.CreateProductAsync(product);
                return Results.Ok(product);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }

        public static async Task<IResult> UpdateProduct(Guid id, Product product, IProductDataService productDataService)
        {
            if (id != product.Id)
            {
                return Results.BadRequest("ID does not match.");
            }

            try
            {
                ModelValidationException.ThrowIfInvalid(product);

                await productDataService.UpdateProductAsync(product);
                return Results.Ok(product);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }

        public static async Task<IResult> DeleteProduct(Guid id, IProductDataService productDataService)
        {
            var allProducts = await productDataService.GetProductsAsync();
            var productToDelete = allProducts.FirstOrDefault(p => p.Id == id);

            if (productToDelete == null)
            {
                return Results.NotFound("Product do not exist.");
            }

            await productDataService.DeleteProductAsync(productToDelete);
            return Results.Ok();
        }
    }
}