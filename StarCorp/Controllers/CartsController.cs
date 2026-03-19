using Microsoft.AspNetCore.Mvc;
using StarCorp.Carts;
using StarCorp.Models;
using System;
using System.Threading.Tasks;

namespace StarCorp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("{cartId}")]
        public async Task<IActionResult> GetCart(Guid cartId)
        {
            var cart = await _cartService.GetCartAsync(cartId);

            if (cart == null)
            {
                return NotFound();
            }

            return Ok(cart);
        }

        [HttpPost("{cartId}/items")]
        public async Task<IActionResult> AddItemToCart(Guid cartId, [FromBody] LineItem item)
        {
            var updatedCart = await _cartService.AddProductToCartAsync(cartId, item);
            return Ok(updatedCart);
        }

        [HttpDelete("{cartId}/items/{productId}")]
        public async Task<IActionResult> RemoveItemFromCart(Guid cartId, Guid productId)
        {
            var updatedCart = await _cartService.RemoveProductFromCartAsync(cartId, productId);

            if (updatedCart == null)
            {
                return NotFound();
            }

            return Ok(updatedCart);
        }
    }
}