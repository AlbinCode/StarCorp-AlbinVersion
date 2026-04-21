using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using StarCorp.Data;
using StarCorp.Exceptions;
using StarCorp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using StarCorp.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace StarCorp.Endpoints
{
    public static class OrderEndpoints
    {
        public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/orders").WithTags("Orders");

            group.MapGet("/", GetOrders);
            group.MapPost("/checkout/{cartId}", Checkout);
            group.MapPut("/{id}", UpdateOrder);
            group.MapDelete("/{id}", DeleteOrder);
        }
        public static async Task<IResult> GetOrders(
            [FromQuery] string? query,
            [FromQuery] int? page,
            [FromQuery] int? pagesize,
            IOrderDataService orderDataService,
            IProductDataService productDataService)
        {
            int currentPage = page ?? 1;
            int currentSize = pagesize ?? 20;

            var allOrders = await orderDataService.GetOrdersAsync();

            if (!string.IsNullOrEmpty(query))
            {
                string lowerQuery = query.ToLower();
                var allProducts = await productDataService.GetProductsAsync();

                var matchingProductIds = allProducts
                 .Where(p => p.Name != null && p.Name.ToLower().Contains(lowerQuery))
                 .Select(p => p.Id)
                 .ToList();

                allOrders = allOrders.Where(o =>
                (o.Buyer != null && o.Buyer.Name != null && o.Buyer.Name.ToLower().Contains(lowerQuery)) ||
                (o.Lines.Any(line => matchingProductIds.Contains(line.ProductId))));
            }

            var result = allOrders
                .Skip((currentPage - 1) * currentSize)
                .Take(currentSize)
                .ToList();

            return Results.Ok(result);
        }

        public static async Task<IResult> Checkout(
           Guid cartId,
           [FromBody] Order orderDetails,
           ICartService cartService,
           IOrderDataService orderDataService,
           ILogger<Order> logger)
        {
            var cart = await cartService.GetCartAsync(cartId);

            if (cart == null || !cart.LineItems.Any())
            {
                return Results.BadRequest("Cart is missing or empty. Cannot create an order.");
            }

            orderDetails.Id = Guid.NewGuid();

            if (orderDetails.Buyer != null)
            {
                if (orderDetails.Buyer.Id == Guid.Empty)
                {
                    orderDetails.Buyer.Id = Guid.NewGuid();
                }
                orderDetails.BuyerId = orderDetails.Buyer.Id;
            }

            orderDetails.Lines = cart.LineItems.Select(item => new LineItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList();

            orderDetails.TotalValue = orderDetails.Lines.Sum(item => item.Price * item.Quantity);

            try
            {
                if (orderDetails.Buyer != null)
                {
                    ModelValidationException.ThrowIfInvalid(orderDetails.Buyer);
                }

                ModelValidationException.ThrowIfInvalid(orderDetails);

                await orderDataService.CreateOrderAsync(orderDetails);
                await cartService.DeleteCartAsync(cartId);

                var mailProperties = new
                {
                    Buyer = orderDetails.Buyer,
                    Email = orderDetails.Buyer.Email,
                    OrderId = orderDetails.Id,
                    DeliveryAddress = orderDetails.Buyer.DeliveryAddress,
                    TotalValue = orderDetails.TotalValue
                };

                try
                {
                    using var httpClient = new System.Net.Http.HttpClient();
                    string functionUrl = "http://localhost:7071/api/SendOrderConfirmation";
                    await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(httpClient, functionUrl, mailProperties);
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to trigger email function {0}", ex.Message);
                }

                return Results.Ok(orderDetails);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }

        public static async Task<IResult> UpdateOrder(Guid id, [FromBody] Order order, IOrderDataService orderDataService)
        {
            if (id != order.Id)
            {
                return Results.BadRequest("ID is not the same ID.");
            }

            try
            {
                ModelValidationException.ThrowIfInvalid(order);

                await orderDataService.UpdateOrderAsync(order);
                return Results.Ok($"Order {id} is updated.");
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception)
            {
                throw new ResourceNotFoundException(nameof(Order), order.Id);
            }
        }

        public static async Task<IResult> DeleteOrder(Guid id, IOrderDataService orderDataService)
        {
            try
            {
                await orderDataService.DeleteOrderAsync(id);
                return Results.NoContent();
            }
            catch (Exception)
            {
                throw new ResourceNotFoundException(nameof(Order), id);
            }
        }
    }
}