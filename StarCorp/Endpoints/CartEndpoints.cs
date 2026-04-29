using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using StarCorp.Data;
using StarCorp.Exceptions;
using StarCorp.Models;
using StarCorp.Logger;
using System;
using System.ComponentModel.DataAnnotations;

namespace StarCorp.Endpoints
{
    public static class CartEndpoints
    {
        public static void MapCartEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/carts")
                .WithTags("Carts");

            group.MapGet("/{cartId}", async (Guid cartId, ICartService cartService, IStarCorpLogger<CartEndpointsLog> logger) =>
            {
                if (cartId == Guid.Empty)
                {
                    logger.LogWarning("GetCart endpoint called with an empty Cart ID.");
                    return Results.BadRequest("Cart ID cannot be empty.");
                }

                logger.LogInformation("Retrieving Cart {CartId}", cartId);

                try
                {
                    var cart = await cartService.GetCartAsync(cartId);
                    return Results.Ok(cart);
                }
                catch (ResourceNotFoundException ex)
                {
                    logger.LogWarning(ex, "Cart not found during GetCart request.");
                    return Results.NotFound(ex.Message);
                }
            });

            group.MapPost("/{cartId}/items", async (Guid cartId, [FromBody] LineItem item, ICartService cartService, IStarCorpLogger<CartEndpointsLog> logger) =>
            {
                if (cartId == Guid.Empty)
                {
                    logger.LogWarning("AddProductToCart endpoint called with an empty Cart ID.");
                    return Results.BadRequest("Cart ID cannot be empty.");
                }

                logger.LogInformation("Adding Item {ProductId} to Cart {CartId}", item.ProductId, cartId);

                try
                {
                    ModelValidationException.ThrowIfInvalid(item);
                    await cartService.AddProductToCartAsync(cartId, item);
                    return Results.Ok();
                }
                catch (ValidationException ex)
                {
                    logger.LogWarning(ex, "Model validation failed when adding item to cart.");
                    return Results.BadRequest(ex.Message);
                }
                catch (ArgumentException ex)
                {
                    logger.LogWarning(ex, "Business rule validation failed when adding item to cart.");
                    return Results.BadRequest(ex.Message);
                }
                catch (ResourceNotFoundException ex)
                {
                    logger.LogWarning(ex, "Resource not found when adding item to cart.");
                    return Results.NotFound(ex.Message);
                }
            });

            group.MapDelete("/{cartId}/items/{productId}", async (Guid cartId, Guid productId, ICartService cartService, IStarCorpLogger<CartEndpointsLog> logger) =>
            {
                if (cartId == Guid.Empty || productId == Guid.Empty)
                {
                    logger.LogWarning("RemoveProductFromCart endpoint called with empty IDs.");
                    return Results.BadRequest("Cart ID and Product ID cannot be empty.");
                }

                logger.LogInformation("Removing Item {ProductId} from Cart {CartId}", productId, cartId);

                try
                {
                    await cartService.RemoveProductFromCartAsync(cartId, productId);
                    return Results.Ok();
                }
                catch (ResourceNotFoundException ex)
                {
                    logger.LogWarning(ex, "Cart or Product not found during removal request.");
                    return Results.NotFound(ex.Message);
                }
            });
        }
    }

    public record CartEndpointsLog;
}
