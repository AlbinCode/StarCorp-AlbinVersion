using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StarCorp.Data;
using StarCorp.Models;

namespace StarCorp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IProductDataService _productDataService;
        private readonly IOrderDataService _orderDataService;

        public ProductsController(ILogger<ProductsController> logger, IProductDataService productDataService, IOrderDataService orderDataService)
        {
            _logger = logger;
            _productDataService = productDataService;
            _orderDataService = orderDataService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var products = await _productDataService.GetProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve products");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            if (product == null) return BadRequest();

            try
            {
                // TODO: Wire up AddProductAsync
                product.Id = Guid.NewGuid(); // Temp ID generation
                return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Product product)
        {
            if (product == null || id != product.Id) return BadRequest("Invalid product data");

            try
            {
                // TODO: Implement update logic
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // TODO: Implement delete logic
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}