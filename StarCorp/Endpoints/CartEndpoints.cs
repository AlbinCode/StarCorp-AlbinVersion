using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using StarCorp.Data;
using StarCorp.Exceptions;
using StarCorp.Models;
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

            group.MapGet("/{cartId}", async (Guid cartId, ICartService cartService) =>
            {
                if (cartId == Guid.Empty) return Results.BadRequest("Cart ID cannot be empty.");

                try
                {
                    var cart = await cartService.GetCartAsync(cartId);
                    return Results.Ok(cart);
                }
                catch (ResourceNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            });

            group.MapPost("/{cartId}/items", async (Guid cartId, [FromBody] LineItem item, ICartService cartService) =>
            {
                if (cartId == Guid.Empty) return Results.BadRequest("Cart ID cannot be empty.");

                try
                {
                    ModelValidationException.ThrowIfInvalid(item);

                    await cartService.AddProductToCartAsync(cartId, item);
                    return Results.Ok();
                }
                catch (ValidationException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
                catch (ResourceNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            });

            group.MapDelete("/{cartId}/items/{productId}", async (Guid cartId, Guid productId, ICartService cartService) =>
            {
                if (cartId == Guid.Empty) return Results.BadRequest("Cart ID cannot be empty.");
                if (productId == Guid.Empty) return Results.BadRequest("Product ID cannot be empty.");

                try
                {
                    await cartService.RemoveProductFromCartAsync(cartId, productId);
                    return Results.Ok();
                }
                catch (ResourceNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            });
        }
    }
}