using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using StarCorp.Data;
using StarCorp.Exceptions;
using StarCorp.Logger;
using StarCorp.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StarCorp.Endpoints
{
    public record OrderEndpointsLog;

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
            IProductDataService productDataService,
            IStarCorpLogger<OrderEndpointsLog> logger)
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
           IStarCorpLogger<OrderEndpointsLog> logger)
        {
            var cart = await cartService.GetCartAsync(cartId);

            if (cart == null || !cart.LineItems.Any())
            {
                logger.LogWarning("Checkout failed Cart {CartId} is missing or empty.", cartId);
                return Results.BadRequest("Cart is missing or empty. Cannot create an order.");
            }

            logger.LogInformation("Processing checkout for Cart {CartId}", cartId);

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
                    Email = orderDetails.Buyer?.Email,
                    OrderId = orderDetails.Id,
                    DeliveryAddress = orderDetails.Buyer?.DeliveryAddress,
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
                    logger.LogError(ex, "Failed to trigger email function for Order {OrderId}", orderDetails.Id);
                }

                return Results.Ok(orderDetails);
            }
            catch (ValidationException ex)
            {
                logger.LogWarning(ex, "Exception caught, validation did not succeed with Cart {CartId}", cartId);
                return Results.BadRequest(ex.Message);
            }
        }

        public static async Task<IResult> UpdateOrder(
            Guid id,
            [FromBody] Order order,
            IOrderDataService orderDataService,
            IStarCorpLogger<OrderEndpointsLog> logger)
        {
            if (id != order.Id)
            {
                logger.LogWarning("UpdateOrder failed ID {id} does not match Body ID {order.Id}.", id, order.Id);
                return Results.BadRequest("ID is not the same ID.");
            }

            logger.LogInformation("Updating Order {OrderId}", id);

            try
            {
                ModelValidationException.ThrowIfInvalid(order);

                await orderDataService.UpdateOrderAsync(order);
                return Results.Ok($"Order {id} is updated.");
            }
            catch (ValidationException ex)
            {
                logger.LogWarning(ex, "Model validation failed when updating Order {OrderId}.", id);
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Exception caught when updating Order {OrderId}. NotFound.", id);
                throw new ResourceNotFoundException(nameof(Order), order.Id);
            }
        }

        public static async Task<IResult> DeleteOrder(
            Guid id,
            IOrderDataService orderDataService,
            IStarCorpLogger<OrderEndpointsLog> logger)
        {
            logger.LogInformation("Deleting Order {OrderId}", id);

            try
            {
                await orderDataService.DeleteOrderAsync(id);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Exception caught when deleting Order {OrderId}. NotFound.", id);
                throw new ResourceNotFoundException(nameof(Order), id);
            }
        }
    }
}