using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StarCorp.Data;

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
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        public async Task<IActionResult> Update()
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            throw new NotImplementedException();
        }
    }
}