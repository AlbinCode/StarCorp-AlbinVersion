using Microsoft.EntityFrameworkCore;
using StarCorp.Abstractions;
using StarCorp.Exceptions;
using StarCorp.Logger;
using StarCorp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StarCorp.Data
{
    public interface IOrderDataService
    {
        Task<IQueryable<IOrder>> GetOrdersAsync();
        Task<Guid> CreateOrderAsync(IOrder order);
        Task<Guid> UpdateOrderAsync(IOrder order);
        Task<Guid> DeleteOrderAsync(Guid id);
    }

    /// <summary>
    /// Simple CSV data service to save orders.
    /// </summary>
    public class OrderDataService : IOrderDataService
    {
        private readonly AppDbContext _context;
        private readonly IStarCorpLogger<OrderDataService> _logger;
        public OrderDataService(AppDbContext context, IStarCorpLogger<OrderDataService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IQueryable<IOrder>> GetOrdersAsync()
        {
            var orders = await _context.Orders.Include(o => o.Lines).ToListAsync();
            return orders.AsQueryable();
        }
        public async Task<Guid> CreateOrderAsync(IOrder order)
        {
            var newOrder = (Order)order;

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New order {OrderId} created successfully.", newOrder.Id);

            return newOrder.Id;
        }
        public async Task<Guid> UpdateOrderAsync(IOrder order)
        {
            var exists = await _context.Orders.AnyAsync(o => o.Id == order.Id);

            if (!exists)
            {
                throw new ResourceNotFoundException(nameof(Order), order.Id);
            }

            var updatedOrder = (Order)order;

            _context.Orders.Update(updatedOrder);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} updated successfully.", updatedOrder.Id);

            return updatedOrder.Id;
        }
        public async Task<Guid> DeleteOrderAsync(Guid id)
        {
            var orderToDelete = await _context.Orders.FindAsync(id);

            if (orderToDelete != null)
            {
                _context.Orders.Remove(orderToDelete);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} deleted successfully.", id);
            }

            return id;
        }
    }
}