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
            // Hämta alla produkter asynkront
            var allProducts = await _productDataService.GetProductsAsync();

            // Om sökord finns, filtrera listan
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
            // Om produkten saknar ID, skapa ett nytt
            if (product.Id == Guid.Empty)
            {
                product.Id = Guid.NewGuid();
            }

            try
            {
                // Försök spara produkten
                await _productDataService.CreateProductAsync(product);
                return Ok(product);
            }
            catch (ArgumentException ex)
            {
                // Om något gick fel, skicka tillbaka felet
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Product product)
        {
            // Kolla så att ID i länken stämmer med ID på produkten
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
                // Om jag försöker uppdatera en produkt som inte finns
                return NotFound("Kunde inte hitta produkten.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Jag måste hämta listan först för att hitta hela produktobjektet
            var allProducts = await _productDataService.GetProductsAsync();

            // Hitta produkten som har rätt ID
            var productToDelete = allProducts.FirstOrDefault(p => p.Id == id);

            if (productToDelete == null)
            {
                return NotFound("Produkten finns inte.");
            }

            // Ta bort den via servicen
            await _productDataService.DeleteProductAsync(productToDelete);

            return Ok();
        }
    }
}