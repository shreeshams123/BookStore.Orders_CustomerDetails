using BookStore.Orders.Interfaces;
using BookStore.Orders.Model.DTOs;
using BookStore.Orders.Model.Entities;
using BookStore.Orders.Model;
using BookStore.Orders.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using MongoDB.Bson;

namespace BookStore.Orders.Tests
{
    [TestFixture]
    public class OrderServiceTests
    {
        private Mock<IExternalService> _mockExternalService;
        private Mock<TokenService> _mockTokenService;
        private Mock<IOrderRepo> _mockOrderRepo;
        private OrderService _orderService;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        [SetUp]
        public void Setup()
        {
            _mockExternalService = new Mock<IExternalService>();
            _mockOrderRepo = new Mock<IOrderRepo>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockTokenService = new Mock<TokenService>(_mockConfiguration.Object, _mockHttpContextAccessor.Object);
            _orderService = new OrderService(_mockExternalService.Object, _mockTokenService.Object, _mockOrderRepo.Object);

            var mockHttpContext = new Mock<HttpContext>();
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            var mockClaim = new Claim(ClaimTypes.NameIdentifier, "123");

            mockClaimsPrincipal.Setup(cp => cp.FindFirst(ClaimTypes.NameIdentifier)).Returns(mockClaim);
            mockHttpContext.Setup(ctx => ctx.User).Returns(mockClaimsPrincipal.Object);
            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext.Object);

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(123);
        }

        [Test]
        public async Task AddToOrderAsync_CartIsEmpty_ReturnsEmptyCartMessage()
        {
            var orderRequestDto = new AddToOrderRequestDto { AddressId = "1" };
            string token = "valid-token";

            _mockTokenService.Setup(x => x.GetUserIdFromToken()).Returns(1);
            _mockExternalService.Setup(x => x.GetItemsFromCart(It.IsAny<string>())).ReturnsAsync(new CartDataDtoList { CartItems = new List<CartDataDto>() });

            var result = await _orderService.AddToOrderAsync(orderRequestDto, token);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Cart is empty", result.Message);
        }

        [Test]
        public async Task AddToOrderAsync_BookNotFound_ReturnsBookNotFoundMessage()
        {
            var orderRequestDto = new AddToOrderRequestDto { AddressId = "1" };
            string token = "valid-token";
            var cartItem = new CartDataDto { BookId = 1, Quantity = 1 };

            _mockTokenService.Setup(x => x.GetUserIdFromToken()).Returns(1);
            _mockExternalService.Setup(x => x.GetItemsFromCart(It.IsAny<string>())).ReturnsAsync(new CartDataDtoList { CartItems = new List<CartDataDto> { cartItem } });
            _mockExternalService.Setup(x => x.BookExists(cartItem.BookId)).ReturnsAsync((BookDataDto)null);

            var result = await _orderService.AddToOrderAsync(orderRequestDto, token);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Book with ID 1 not found!", result.Message);
        }

        [Test]
        public async Task AddToOrderAsync_StockUpdateFails_ReturnsStockUpdateFailureMessage()
        {
            var orderRequestDto = new AddToOrderRequestDto { AddressId = "1" };
            string token = "valid-token";
            var cartItem = new CartDataDto { BookId = 1, Quantity = 1 };
            var bookDetails = new BookDataDto { BookId = 1, Title = "Book Title", Price = 19.99m, StockQuantity = 5 };

            _mockTokenService.Setup(x => x.GetUserIdFromToken()).Returns(1);
            _mockExternalService.Setup(x => x.GetItemsFromCart(It.IsAny<string>())).ReturnsAsync(new CartDataDtoList { CartItems = new List<CartDataDto> { cartItem } });
            _mockExternalService.Setup(x => x.BookExists(cartItem.BookId)).ReturnsAsync(bookDetails);
            _mockExternalService.Setup(x => x.UpdateBookStockAsync(cartItem.BookId, bookDetails.StockQuantity - cartItem.Quantity, token))
                .ReturnsAsync(new ApiResponse<BookDataDto> { Success = false, Message = "Stock update failed" });

            var result = await _orderService.AddToOrderAsync(orderRequestDto, token);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Failed to update stock for book ID 1: Stock update failed", result.Message);
        }

        [Test]
        public async Task AddToOrderAsync_SuccessfulOrder_ReturnsSuccessMessage()
        {
            var orderRequestDto = new AddToOrderRequestDto { AddressId = "1" };
            string token = "valid-token";
            var cartItem = new CartDataDto { BookId = 1, Quantity = 1 };
            var bookDetails = new BookDataDto { BookId = 1, Title = "Book Title", Price = 19.99m, StockQuantity = 5 };

            _mockTokenService.Setup(x => x.GetUserIdFromToken()).Returns(1);
            _mockExternalService.Setup(x => x.GetItemsFromCart(It.IsAny<string>())).ReturnsAsync(new CartDataDtoList { CartItems = new List<CartDataDto> { cartItem } });
            _mockExternalService.Setup(x => x.BookExists(cartItem.BookId)).ReturnsAsync(bookDetails);
            _mockExternalService.Setup(x => x.UpdateBookStockAsync(cartItem.BookId, bookDetails.StockQuantity - cartItem.Quantity, token)).ReturnsAsync(new ApiResponse<BookDataDto> { Success = true });
            var clearCartResponse = new ClearCartResponse
            {
                Success = true,
                Message = "Cart item deleted successfully"
            };

            _mockExternalService.Setup(x => x.DeleteFromCart(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(clearCartResponse);

            _mockOrderRepo.Setup(x => x.AddOrderAsync(It.IsAny<Order>())).ReturnsAsync(new ApiResponse<Order> { Success = true, Message = "Order created successfully" });

            var result = await _orderService.AddToOrderAsync(orderRequestDto, token);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Order created successfully", result.Message);
        }

        [Test]
        public async Task DeleteOrderAsync_SuccessfulDeletion_ReturnsSuccessMessage()
        {
            string orderId = "order123";
            int userId = 123;

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);

            var order = new Order { Id = new ObjectId("60b8d2bfa14e6d3f4f8b4567"), UserId = userId };
            _mockOrderRepo.Setup(repo => repo.GetOrderByIdAsync(orderId, userId))
                .ReturnsAsync(new ApiResponse<Order> { Success = false, Message = "Order deleted successfully" });

            _mockOrderRepo.Setup(repo => repo.DeleteOrderFromDbAsync(orderId, userId))
                .ReturnsAsync(new ApiResponse<Order> { Success = true, Message = "Order deleted successfully." });

            var result = await _orderService.DeleteOrderAsync(orderId);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Order deleted successfully.", result.Message);
        }

        [Test]
        public async Task DeleteOrderAsync_FailedDeletion_ReturnsFailureMessage()
        {
            string orderId = "order123";
            int userId = 123;

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);

            var order = new Order { Id = new ObjectId("60b8d2bfa14e6d3f4f8b4567"), UserId = userId };
            _mockOrderRepo.Setup(repo => repo.GetOrderByIdAsync(orderId, userId))
                .ReturnsAsync(new ApiResponse<Order> { Success = false, Message = "Failed to delete the order." });

            _mockOrderRepo.Setup(repo => repo.DeleteOrderFromDbAsync(orderId, userId))
                .ReturnsAsync(new ApiResponse<Order> { Success = false, Message = "Failed to delete the order." });

            var result = await _orderService.DeleteOrderAsync(orderId);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Failed to delete the order.", result.Message);
        }
    }
}
