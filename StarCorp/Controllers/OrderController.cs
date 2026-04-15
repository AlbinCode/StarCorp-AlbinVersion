using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StarCorp.Data;
using StarCorp.Models;
using StarCorp.Carts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StarCorp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IProductDataService _productDataService;
        private readonly IOrderDataService _orderDataService;
        private readonly ICartService _cartService;
        private readonly AppDbContext _context;

        public OrderController(
            ILogger<OrderController> logger,
            IProductDataService productDataService,
            IOrderDataService orderDataService,
            ICartService cartService,
            AppDbContext context)
        {
            _logger = logger;
            _productDataService = productDataService;
            _orderDataService = orderDataService;
            _cartService = cartService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string query = null, [FromQuery] int page = 1, [FromQuery] int pagesize = 20)
        {
            var allOrders = await _orderDataService.GetOrdersAsync();

            if (!string.IsNullOrEmpty(query))
            {
                string lowerQuery = query.ToLower();
                var allProducts = await _productDataService.GetProductsAsync();

                var matchingProductIds = allProducts
                 .Where(p => p.Name != null && p.Name.ToLower().Contains(lowerQuery))
                 .Select(p => p.Id)
                 .ToList();

                allOrders = allOrders.Where(o =>
                (o.Buyer != null && o.Buyer.ToLower().Contains(lowerQuery)) ||
                (o.Lines.Any(line => matchingProductIds.Contains(line.ProductId))));
            }

            var result = allOrders
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToList();

            return Ok(result);
        }

        [HttpPost("checkout/{cartId}")]
        public async Task<IActionResult> Checkout(Guid cartId, [FromBody] Order orderDetails)
        {
            var cart = await _cartService.GetCartAsync(cartId);

            if (cart == null || !cart.LineItems.Any())
            {
                return BadRequest("Cart is missing or empty. Cannot create an order.");
            }

            orderDetails.Id = Guid.NewGuid();

            orderDetails.Lines = cart.LineItems.Select(item => new LineItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList();

            await _orderDataService.CreateOrderAsync(orderDetails);

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            var mailProperties = new
            {
                Buyer = orderDetails.Buyer,
                BuyerEmail = orderDetails.BuyerEmail,
                OrderId = orderDetails.Id,
                DeliveryAddress = orderDetails.DeliveryAddress,
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
                _logger.LogError("Failed to trigger email function {0}", ex.Message);
            }

            return Ok(orderDetails);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] Order order)
        {
            if (id != order.Id)
            {
                return BadRequest("ID in the url is not the same ID.");
            }

            try
            {
                await _orderDataService.UpdateOrderAsync(order);
                return Ok($"Order {id} is updated.");
            }
            catch (Exception)
            {
                return NotFound("Could not find any order with this ID.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _orderDataService.DeleteOrderAsync(id);
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound("Could not find any order with this ID.");
            }
        }
    }
}