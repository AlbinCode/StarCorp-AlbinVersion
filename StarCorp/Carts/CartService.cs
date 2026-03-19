using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StarCorp.Data;
using StarCorp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StarCorp.Carts
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(AppDbContext context, ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Cart?> GetCartAsync(Guid cartId)
        {
            return await _context.Carts
             .Include(c => c.LineItems)
             .ThenInclude(li => li.Product)
             .FirstOrDefaultAsync(c => c.Id == cartId);
        }

        public async Task<Cart> AddProductToCartAsync(Guid cartId, LineItem item)
        {
            var cart = await _context.Carts
                .Include(c => c.LineItems)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
            {
                cart = new Cart { Id = cartId };
                _context.Carts.Add(cart);
            }

            var existingItem = cart.LineItems.FirstOrDefault(i => i.ProductId == item.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
                existingItem.Price = item.Price;
            }
            else
            {
                item.Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
                cart.LineItems.Add(item);
            }

            await _context.SaveChangesAsync();

            return cart;
        }

        public async Task<Cart?> RemoveProductFromCartAsync(Guid cartId, Guid productId)
        {
            var cart = await _context.Carts
                .Include(c => c.LineItems)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null) return null;

            var itemToRemove = cart.LineItems.FirstOrDefault(i => i.ProductId == productId);

            if (itemToRemove != null)
            {
                cart.LineItems.Remove(itemToRemove);
                _context.Remove(itemToRemove);

                await _context.SaveChangesAsync();
            }

            return cart;
        }
    }
}