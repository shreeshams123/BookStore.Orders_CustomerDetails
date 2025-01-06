using BookStore.Orders.Model.Entities;
using BookStore.Orders.Model;
using BookStore.Orders.Model.DTOs;

namespace BookStore.Orders.Interfaces
{
    public interface ICustomerDetailsService

    {
        Task<ApiResponse<List<CustomerResponseDto>>> GetCustomerDetailsByUserIdAsync();
        Task<ApiResponse<CustomerDetailsResponseDto>> AddCustomerDetailsAsync(CustomerDetailsDto customerDetails);
        Task<ApiResponse<CustomerDetails>> DeleteCustomerDetailsAsync(string addressId);
        Task<ApiResponse<CustomerDetails>> UpdateCustomerDetailsAsync(string addressId, CustomerDetailsDto customerDetails);
        Task<ApiResponse<CustomerDetails>> GetAddressByUserIdAsync(string addressId);

    }
}
