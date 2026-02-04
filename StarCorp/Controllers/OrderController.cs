using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StarCorp.Data;
using StarCorp.Models;
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

        public OrderController(ILogger<OrderController> logger, IProductDataService productDataService, IOrderDataService orderDataService)
        {
            _logger = logger;
            _productDataService = productDataService;
            _orderDataService = orderDataService;
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Order order)
        {
            if (order == null)
            {
                return BadRequest("Ordern saknas.");
            }

            if (order.Lines == null || !order.Lines.Any())
            {
                return BadRequest("En order måste innehålla minst en produkt.");
            }

            if (order.Id == Guid.Empty)
            {
                order.Id = Guid.NewGuid();
            }

            await _orderDataService.SaveOrder(order);

            return Ok(order);
        }
    }
}
