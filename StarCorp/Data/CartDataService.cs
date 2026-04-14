using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StarCorp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarCorp.Data
{
    public interface ICartService
    {
        Task<Cart?> GetCartAsync(Guid cartId);
        Task<Cart> AddProductToCartAsync(Guid cartId, LineItem item);
        Task<Cart?> RemoveProductFromCartAsync(Guid cartId, Guid productId);
    }

        public class CartDataService : ICartService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CartDataService> _logger;

        public CartDataService(AppDbContext context, ILogger<CartDataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Cart?> GetCartAsync(Guid cartId)
        {
            var cart = await _context.Carts
             .Include(c => c.LineItems)
             .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
            {
                return null;
            }

            foreach (var lineItem in cart.LineItems)
            {
                var product = _context.Products.FirstOrDefault(x => x.Id == lineItem.ProductId);

                if (product != null)
                {
                    lineItem.InStock = product.Stock > 0;
                }
                else
                {
                    lineItem.InStock = false;
                }
            }

            return cart;
            
        }
        public async Task<Cart> AddProductToCartAsync(Guid cartId, LineItem item)
        {
            var product = await _context.Products.FindAsync(item.ProductId);

            if (product == null)
            {
                throw new ArgumentException($"Product with this ID {item.ProductId} does not exist.");
            }

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