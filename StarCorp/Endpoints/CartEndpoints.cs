using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using StarCorp.Data;
using StarCorp.Models;
using System;

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
                var cart = await cartService.GetCartAsync(cartId);

                if (cart == null)
                {
                    return Results.NotFound("Could not find the cart.");
                }

                return Results.Ok(cart);
            });

            group.MapPost("/{cartId}/items", async (Guid cartId, [FromBody] LineItem item, ICartService cartService) =>
            {
                try
                {
                    await cartService.AddProductToCartAsync(cartId, item);
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });

            group.MapDelete("/{cartId}/items/{productId}", async (Guid cartId, Guid productId, ICartService cartService) =>
            {
                try
                {
                    await cartService.RemoveProductFromCartAsync(cartId, productId);
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    return Results.NotFound(ex.Message);
                }
            });
        }
    }
}