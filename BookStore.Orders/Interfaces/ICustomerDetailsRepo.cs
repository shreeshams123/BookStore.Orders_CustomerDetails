using BookStore.Orders.Model.DTOs;
using BookStore.Orders.Model;
using BookStore.Orders.Model.Entities;

namespace BookStore.Orders.Interfaces
{
    public interface ICustomerDetailsRepo
    {
        Task<ApiResponse<List<CustomerResponseDto>>> GetCustomerDetailsByUserIdFromDbAsync(int userId);
        Task<ApiResponse<CustomerDetailsResponseDto>> AddCustomerDetailsFromDbAsync(CustomerDetailsDto customerDetails, int userId);
        Task<ApiResponse<CustomerDetails>> DeleteCustomerDetailsFromDb(string addressId, int userId);
        Task<ApiResponse<CustomerDetails>> UpdateCustomerDetailsAsyncFromDb(string addressId, CustomerDetailsDto updatedCustomerDetails, int userId);
        Task<ApiResponse<CustomerDetails>> GetAddressByUserIdFromDbAsync(string addressId, int userId);
    }
}
