using Microsoft.EntityFrameworkCore;
using StarCorp.Exceptions;
using StarCorp.Models;
using StarCorp.Logger;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StarCorp.Data
{
    public interface ICartService
    {
        Task<Cart?> GetCartAsync(Guid cartId);
        Task<Cart> AddProductToCartAsync(Guid cartId, LineItem item);
        Task<Cart?> RemoveProductFromCartAsync(Guid cartId, Guid productId);
        Task DeleteCartAsync(Guid cartId);
    }

    public class CartDataService : ICartService
    {
        private readonly AppDbContext _context;
        private readonly IStarCorpLogger<CartDataService> _logger;

        public CartDataService(AppDbContext context, IStarCorpLogger<CartDataService> logger)
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
                throw new ResourceNotFoundException(nameof(Cart), cartId);
            }

            return cart;
        }
        public async Task<Cart> AddProductToCartAsync(Guid cartId, LineItem item)
        {
            var product = await _context.Products.FindAsync(item.ProductId);

            if (product == null)
            {
                throw new ResourceNotFoundException(nameof(Product), item.ProductId);
            }

            if (product.Price <= 0)
            {
                _logger.LogError("Failed to add product to cart: Product {ProductName} has no price.", product.Name);
                throw new ArgumentException($"Product {product.Name} doesn't have a correct price and cannot be added to cart.");
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
                existingItem.Price = product.Price;
            }
            else
            {
                item.Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
                item.Price = product.Price;
                cart.LineItems.Add(item);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} added to Cart {CartId}.", item.ProductId, cartId);

            return cart;
        }

        public async Task<Cart?> RemoveProductFromCartAsync(Guid cartId, Guid productId)
        {
            var cart = await _context.Carts
                .Include(c => c.LineItems)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
            {
                throw new ResourceNotFoundException(nameof(Cart), cartId);
            }

            var itemToRemove = cart.LineItems.FirstOrDefault(i => i.ProductId == productId);

            if (itemToRemove != null)
            {
                cart.LineItems.Remove(itemToRemove);
                _context.Remove(itemToRemove);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed Product {ProductId} from Cart {CartId}.", productId, cartId);
            }

            return cart;
        }

        public async Task DeleteCartAsync(Guid cartId)
        {
            var cart = await _context.Carts.FindAsync(cartId);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cart {CartId} was permanently removed.", cartId);
            }
        }
    }
}