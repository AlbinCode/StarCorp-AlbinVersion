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
        public async Task<IActionResult> Get([FromQuery] string query = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var allProducts = await _productDataService.GetProductsAsync();

            if (!string.IsNullOrEmpty(query))
            {
                string lowerQuery = query.ToLower();

                allProducts = allProducts.Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(lowerQuery)) ||
                    (p.Description != null && p.Description.ToLower().Contains(lowerQuery)) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(lowerQuery)) ||
                    (p.Category != null && p.Category.ToLower().Contains(lowerQuery))
                );
            }

           
            var result = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            if (product.Id == Guid.Empty)
            {
                product.Id = Guid.NewGuid();
            }

            try
            {
                await _productDataService.CreateProductAsync(product);
                return Ok(product);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Product product)
        {
            if (id != product.Id)
            {
                return BadRequest("ID matchar inte.");
            }

            try
            {
                await _productDataService.UpdateProductAsync(product);
                return Ok(product);
            }
            catch (ArgumentException)
            {
                return NotFound("Kunde inte hitta produkten.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var allProducts = await _productDataService.GetProductsAsync();

            var productToDelete = allProducts.FirstOrDefault(p => p.Id == id);

            if (productToDelete == null)
            {
                return NotFound("Produkten finns inte.");
            }

            await _productDataService.DeleteProductAsync(productToDelete);

            return Ok();
        }
    }
}