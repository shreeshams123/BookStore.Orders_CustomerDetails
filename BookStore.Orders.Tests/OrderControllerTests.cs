using BookStore.Orders.Controllers;
using BookStore.Orders.Interfaces;
using BookStore.Orders.Model;
using BookStore.Orders.Model.DTOs;
using BookStore.Orders.Model.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Moq;

namespace BookStore.Orders.Tests
{
    [TestFixture]
    public class OrderControllerTests
    {
        private Mock<IOrderService> _mockOrderService;
        private Mock<HttpContext> _mockHttpContext;
        private OrderController _orderController;

        [SetUp]
        public void SetUp()
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(x => x.Request.Headers["Authorization"]).Returns("Bearer some-valid-token");

            _mockOrderService = new Mock<IOrderService>();
            _orderController = new OrderController(_mockOrderService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }

        [Test]
        public async Task AddToOrders_ReturnsOk_WhenAddToOrderIsSuccessful()
        {
            var addToOrderRequestDto = new AddToOrderRequestDto
            {
                AddressId = "address123"
            };

            var token = "some-valid-token";

            var apiResponse = new ApiResponse<Order>
            {
                Success = true,
                Message = "Order added successfully",
                Data = new Order
                {
                    Id = new ObjectId(),
                    UserId = 1,
                    OrderItems = new List<OrderItemDto>
                    {
                        new OrderItemDto { BookId = 1, Quantity = 2 },
                        new OrderItemDto { BookId = 2, Quantity = 1 }
                    },
                    TotalQuantity = 3,
                    Date = DateTime.Now,
                    TotalPrice = 49.99m,
                    AddressId = addToOrderRequestDto.AddressId,
                    Status = "Pending"
                }
            };

            _mockOrderService.Setup(service => service.AddToOrderAsync(It.Is<AddToOrderRequestDto>(dto => dto.AddressId == addToOrderRequestDto.AddressId), It.Is<string>(t => t == token)))
                .ReturnsAsync(apiResponse);

            var result = await _orderController.AddToOrders(addToOrderRequestDto);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(apiResponse, okResult.Value);
        }

        [Test]
        public async Task AddToOrders_ReturnsBadRequest_WhenAddToOrderFails()
        {
            var addToOrderRequestDto = new AddToOrderRequestDto
            {
                AddressId = "address123"
            };

            var token = "some-valid-token";

            _mockHttpContext.Setup(x => x.Request.Headers["Authorization"]).Returns($"Bearer {token}");

            var apiResponse = new ApiResponse<Order>
            {
                Success = false,
                Message = "Failed to add order"
            };

            _mockOrderService.Setup(service => service.AddToOrderAsync(It.IsAny<AddToOrderRequestDto>(), It.IsAny<string>()))
                .ReturnsAsync(apiResponse);

            var result = await _orderController.AddToOrders(addToOrderRequestDto);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual(apiResponse, badRequestResult.Value);
        }

        [Test]
        public async Task DeleteOrderAsync_ReturnsOk_WhenDeleteOrderIsSuccessful()
        {
            var orderId = "order123";
            var apiResponse = new ApiResponse<Order> { Success = true, Message = "Order deleted successfully" };

            _mockOrderService.Setup(service => service.DeleteOrderAsync(It.IsAny<string>()))
                .ReturnsAsync(apiResponse);

            var result = await _orderController.DeleteOrderAsync(orderId);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(apiResponse, okResult.Value);
        }

        [Test]
        public async Task DeleteOrderAsync_ReturnsBadRequest_WhenDeleteOrderFails()
        {
            var orderId = "order123";
            var apiResponse = new ApiResponse<Order> { Success = false, Message = "Failed to delete order" };

            _mockOrderService.Setup(service => service.DeleteOrderAsync(It.IsAny<string>()))
                .ReturnsAsync(apiResponse);

            var result = await _orderController.DeleteOrderAsync(orderId);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual(apiResponse, badRequestResult.Value);
        }

        [Test]
        public async Task GetAllOrders_ReturnsOk_WhenGetAllOrdersIsSuccessful()
        {
            var token = "some-token";

            var apiResponse = new ApiResponse<List<OrderResponseDto>>
            {
                Success = true,
                Message = "Orders fetched successfully",
                Data = new List<OrderResponseDto>
                {
                    new OrderResponseDto
                    {
                        Id = "order123",
                        UserId = 1,
                        OrderItems = new List<OrderDataDto>
                        {
                            new OrderDataDto
                            {
                                BookId = 1,
                                Quantity = 2,
                                title = "Book 1",
                                BookPrice = 19.99m,
                                Author = "Author 1",
                                Image = "book1.jpg"
                            },
                            new OrderDataDto
                            {
                                BookId = 2,
                                Quantity = 1,
                                title = "Book 2",
                                BookPrice = 29.99m,
                                Author = "Author 2",
                                Image = "book2.jpg"
                            }
                        },
                        TotalQuantity = 3,
                        Date = DateTime.Now,
                        TotalPrice = 69.97m,
                        AddressId = "address123",
                        Status = "Pending"
                    }
                }
            };

            _mockOrderService.Setup(service => service.GetAllOrdersAsync(It.IsAny<string>()))
                .ReturnsAsync(apiResponse);

            var result = await _orderController.GetAllOrders();

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var response = okResult.Value as ApiResponse<List<OrderResponseDto>>;
            Assert.NotNull(response);
            Assert.AreEqual(apiResponse.Success, response.Success);
            Assert.AreEqual(apiResponse.Message, response.Message);
            Assert.AreEqual(apiResponse.Data.Count, response.Data.Count);
            Assert.AreEqual(apiResponse.Data[0].Id, response.Data[0].Id);
            Assert.AreEqual(apiResponse.Data[0].OrderItems.Count, response.Data[0].OrderItems.Count);
        }
    }
}
