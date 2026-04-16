using FakeItEasy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StarCorp.Abstractions;
using StarCorp.Data;
using StarCorp.Endpoints;
using StarCorp.Models;
using Xunit;


namespace StarCorp.Tests.Endpoints
{
    public class OrderEndpointsTests
    {
        private IProductDataService _productDataService;
        private IOrderDataService _orderDataService;

        public OrderEndpointsTests()
        {
            _productDataService = A.Fake<IProductDataService>();
            _orderDataService = A.Fake<IOrderDataService>();
        }

        [Fact]
        public async Task Checkout_ShouldReturnOk_WhenCartIsValid()
        {
            var cartId = Guid.NewGuid();
            var orderDetails = new Order { Buyer = "Test-Kalle", BuyerEmail = "test@test.com" };

            var fakeCart = new Cart
            {
                Id = cartId,
                LineItems = new List<LineItem>
                {
                    new LineItem { ProductId = Guid.NewGuid(), Quantity = 1, Price = 1500m }
                }
            };

            var fakeCartService = A.Fake<ICartService>();
            A.CallTo(() => fakeCartService.GetCartAsync(cartId))
                .Returns(Task.FromResult<Cart?>(fakeCart));

            var fakeLogger = A.Fake<ILogger<Order>>();

            A.CallTo(() => _orderDataService.CreateOrderAsync(A<IOrder>._))
                .Returns(Task.FromResult(Guid.NewGuid()));

            var result = await OrderEndpoints.Checkout(
                cartId,
                orderDetails,
                fakeCartService,
                _orderDataService,
                fakeLogger);

            var okResult = Assert.IsType<Ok<Order>>(result);
            Assert.Equal("Test-Kalle", okResult.Value.Buyer);

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

            var result = await OrderEndpoints.GetOrders(null, null, null, _orderDataService, _productDataService);

            var okResult = Assert.IsType<Ok<List<IOrder>>>(result);
            Assert.Equal(2, okResult.Value.Count);
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

            var result = await OrderEndpoints.UpdateOrder(orderId, updatedOrder, _orderDataService);

            var okResult = Assert.IsType<Ok<string>>(result);
            Assert.Equal($"Order {orderId} is updated.", okResult.Value);

            A.CallTo(() => _orderDataService.UpdateOrderAsync(A<IOrder>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenOrderExists()
        {
            var orderId = Guid.NewGuid();

            A.CallTo(() => _orderDataService.DeleteOrderAsync(A<Guid>._))
                .Returns(Task.FromResult(orderId));

            var result = await OrderEndpoints.DeleteOrder(orderId, _orderDataService);

            Assert.IsType<NoContent>(result);

            A.CallTo(() => _orderDataService.DeleteOrderAsync(A<Guid>._))
                .MustHaveHappenedOnceExactly();
        }
    }
}