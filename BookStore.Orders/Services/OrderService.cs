using BookStore.Orders.Interfaces;
using BookStore.Orders.Model;
using BookStore.Orders.Model.DTOs;
using BookStore.Orders.Model.Entities;

namespace BookStore.Orders.Services
{
    public class OrderService:IOrderService
    {
        private readonly IExternalService _externalService;
        private readonly TokenService _tokenService;
        private readonly IOrderRepo _orderRepo;
        public OrderService(IExternalService externalService,TokenService tokenService,IOrderRepo orderRepo) { 
            _externalService = externalService;
            _tokenService = tokenService;
            _orderRepo = orderRepo;

        }
        public async Task<ApiResponse<Order>> AddToOrderAsync(AddToOrderRequestDto orderRequestDto, string token)
        {
            int userId = _tokenService.GetUserIdFromToken();
            var cartData = await _externalService.GetItemsFromCart(token);
            if (cartData == null || cartData.CartItems.Count == 0)
            {
                return new ApiResponse<Order>
                {
                    Success = false,
                    Message = "Cart is empty"
                };
            }

            var order = new Order
            {
                UserId = userId,
                OrderItems = new List<OrderItemDto>(),
                TotalQuantity = 0,
                Date = DateTime.UtcNow,
                TotalPrice = 0,
                AddressId = orderRequestDto.AddressId, 
                Status = "Pending"
            };

            foreach (var cartItem in cartData.CartItems)
            {
                var bookDetails = await _externalService.BookExists(cartItem.BookId);
                if (bookDetails == null)
                {
                    return new ApiResponse<Order>
                    {
                        Success = false,
                        Message = $"Book with ID {cartItem.BookId} not found!"
                    };
                }

                order.OrderItems.Add(new OrderItemDto
                {
                    BookId = cartItem.BookId,
                    Quantity = cartItem.Quantity
                });

                order.TotalQuantity += cartItem.Quantity;
                order.TotalPrice += bookDetails.Price * cartItem.Quantity;
                var stockUpdateResult = await _externalService.UpdateBookStockAsync(cartItem.BookId,bookDetails.StockQuantity- cartItem.Quantity,token);
                if (!stockUpdateResult.Success)
                {
                    return new ApiResponse<Order>
                    {
                        Success = false,
                        Message = $"Failed to update stock for book ID {cartItem.BookId}: {stockUpdateResult.Message}"
                    };
                }

                await _externalService.DeleteFromCart(userId, cartItem.BookId, token);
            }

            return await _orderRepo.AddOrderAsync(order);
        }
        public async Task<ApiResponse<Order>> DeleteOrderAsync(string orderId)
        {
            int userId = _tokenService.GetUserIdFromToken();

            var order = await _orderRepo.GetOrderByIdAsync(orderId, userId);
            if (order == null)
            {
                return new ApiResponse<Order>
                {
                    Success = false,
                    Message = "Order not found or you are not authorized to delete it."
                };
            }

            var deleteResult = await _orderRepo.DeleteOrderFromDbAsync(orderId, userId);
            if (deleteResult.Success)
            {
                return new ApiResponse<Order>
                {
                    Success = true,
                    Message = "Order deleted successfully."
                };
            }

            return new ApiResponse<Order>
            {
                Success = false,
                Message = "Failed to delete the order."
            };
        }
        public async Task<ApiResponse<List<OrderResponseDto>>> GetAllOrdersAsync(string token)
        {
            try
            {
                int userId = _tokenService.GetUserIdFromToken();

                var ordersResponse = await _orderRepo.GetAllOrdersFromDbAsync(userId);

                if (!ordersResponse.Success)
                {
                    return new ApiResponse<List<OrderResponseDto>>
                    {
                        Success = false,
                        Message = ordersResponse.Message
                    };
                }

                var enrichedOrders = new List<OrderResponseDto>();

                foreach (var order in ordersResponse.Data)
                {
                    var orderDto = new OrderResponseDto
                    {
                        Id = order.Id.ToString(),
                        UserId = order.UserId,
                        TotalQuantity = order.TotalQuantity,
                        Date = order.Date,
                        TotalPrice = order.TotalPrice,
                        AddressId = order.AddressId,
                        Status = order.Status,
                        OrderItems = new List<OrderDataDto>()
                    };

                    foreach (var orderItem in order.OrderItems)
                    {
                        var bookDetails = await _externalService.BookExists(orderItem.BookId);

                        if (bookDetails != null)
                        {
                            orderDto.OrderItems.Add(new OrderDataDto
                            {
                                BookId = orderItem.BookId,
                                Quantity = orderItem.Quantity,
                                title=bookDetails.Title,
                                BookPrice= bookDetails.Price,
                                Author = bookDetails.Author,
                                Image = bookDetails.Image 
                            });
                        }
                    }

                    enrichedOrders.Add(orderDto);
                }

                return new ApiResponse<List<OrderResponseDto>>
                {
                    Success = true,
                    Message = "Orders retrieved successfully.",
                    Data = enrichedOrders
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<OrderResponseDto>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving orders: {ex.Message}"
                };
            }
        }


    }
}
