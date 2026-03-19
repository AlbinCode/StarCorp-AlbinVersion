using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StarCorp.Controllers;
using StarCorp.Data;
using StarCorp.Models;
using StarCorp.Abstractions;
using Xunit;


namespace StarCorp.Tests.Controllers
{
    public class OrderControllerTests
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IProductDataService _productDataService;
        private readonly IOrderDataService _orderDataService;
        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _logger = A.Fake<ILogger<OrderController>>();
            _productDataService = A.Fake<IProductDataService>();
            _orderDataService = A.Fake<IOrderDataService>();

            _controller = new OrderController(_logger, _productDataService, _orderDataService);
        }

        [Fact]
        public async Task Create_ShouldReturnOk_WhenOrderIsValid()
        {
            var testOrder = new Order
            {
                Id = Guid.NewGuid(),
                Buyer = "Test-Kalle",
                Lines = new List<LineItem>
                {
                    new LineItem { ProductId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            A.CallTo(() => _orderDataService.CreateOrderAsync(A<IOrder>._))
                .Returns(Task.FromResult(testOrder.Id));

            var result = await _controller.Create(testOrder);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedOrder = Assert.IsType<Order>(okResult.Value);
            Assert.Equal("Test-Kalle", returnedOrder.Buyer);

            A.CallTo(() => _orderDataService.CreateOrderAsync(A<IOrder>._))
                .MustHaveHappenedOnceExactly();

        }

        [Fact]
        public async Task Get_ShouldReturnAllOrders_WhenCalled()
        {

            var experimentOrders = new List<IOrder>
            {
                new Order { Id = Guid.NewGuid(), Buyer = "Kalle" },
                new Order { Id = Guid.NewGuid(), Buyer = "Lisa" }
            };

            A.CallTo(() => _orderDataService.GetOrdersAsync())
        .Returns(Task.FromResult(experimentOrders.AsQueryable()));

            var result = await _controller.Get();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedList = Assert.IsType<List<IOrder>>(okResult.Value);

            Assert.Equal(2, returnedList.Count);
        }

        [Fact]
        public async Task Update_ShouldReturnOk_WhenUpdateIsSuccesful()
        {
            var orderId = Guid.NewGuid();
            var updatedOrder = new Order
            {
                Id = orderId,
                Buyer = "Uppdaterad Kalle",
                Lines = new List<LineItem>()
            };

            A.CallTo(() => _orderDataService.UpdateOrderAsync(A<IOrder>._))
               .Returns(Task.FromResult(updatedOrder.Id));

            var result = await _controller.UpdateOrder(orderId, updatedOrder);

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal($"Order {orderId} är uppdaterad.", okResult.Value);

            A.CallTo(() => _orderDataService.UpdateOrderAsync(A<IOrder>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenOrderExists()
        {
            var orderId = Guid.NewGuid();
            var orderToDelete = new Order { Id = orderId, Buyer = "Ska Bort" };

            var existingOrders = new List<IOrder> { orderToDelete };

            A.CallTo(() => _orderDataService.GetOrdersAsync())
                .Returns(Task.FromResult(existingOrders.AsQueryable()));

            A.CallTo(() => _orderDataService.DeleteOrderAsync(A<Guid>._))
                .Returns(Task.FromResult(Guid.NewGuid()));

            var result = await _controller.Delete(orderId);

            Assert.IsType<NoContentResult>(result);

            A.CallTo(() => _orderDataService.DeleteOrderAsync(A<Guid>._))
                .MustHaveHappenedOnceExactly();

        }
    }
}