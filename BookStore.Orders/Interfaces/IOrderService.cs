using BookStore.Orders.Model.DTOs;
using BookStore.Orders.Model.Entities;
using BookStore.Orders.Model;

namespace BookStore.Orders.Interfaces
{
    public interface IOrderService
    {
        Task<ApiResponse<Order>> AddToOrderAsync(AddToOrderRequestDto orderRequestDto, string token);
        Task<ApiResponse<List<OrderResponseDto>>> GetAllOrdersAsync(string token);
        Task<ApiResponse<Order>> DeleteOrderAsync(string orderId);
    }
}
