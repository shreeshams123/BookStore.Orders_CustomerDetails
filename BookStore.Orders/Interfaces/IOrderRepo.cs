using BookStore.Orders.Model;
using BookStore.Orders.Model.DTOs;
using BookStore.Orders.Model.Entities;

namespace BookStore.Orders.Interfaces
{
    public interface IOrderRepo
    {
        Task<ApiResponse<Order>> AddOrderAsync(Order order);
        Task<ApiResponse<Order>> GetOrderByIdAsync(string orderId, int userId);
        Task<ApiResponse<List<Order>>> GetAllOrdersFromDbAsync(int userId);
        Task<ApiResponse<Order>> DeleteOrderFromDbAsync(string orderId, int userId);

    }
}
