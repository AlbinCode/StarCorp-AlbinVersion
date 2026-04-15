using StarCorp.Models;
using System;
using System.Threading.Tasks;

namespace StarCorp.Carts
{
    public interface ICartService
    {
        Task<Cart?> GetCartAsync(Guid cartId);
        Task<Cart> AddProductToCartAsync(Guid cartId, LineItem item);
        Task<Cart?> RemoveProductFromCartAsync(Guid cartId, Guid productId);

    }
}
