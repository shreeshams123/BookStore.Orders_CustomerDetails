using BookStore.Orders.Interfaces;
using BookStore.Orders.Model;
using BookStore.Orders.Model.DTOs;
using BookStore.Orders.Model.Entities;
using System.Diagnostics.Contracts;

namespace BookStore.Orders.Services
{
    public class CustomerDetailsService:ICustomerDetailsService
    {
        private readonly ICustomerDetailsRepo _customerRepo;
        private readonly TokenService _tokenService;
        public CustomerDetailsService(TokenService tokenService,ICustomerDetailsRepo customerRepo)
        {
            _customerRepo = customerRepo;
            _tokenService = tokenService;
            
        }
        public async Task<ApiResponse<CustomerDetailsResponseDto>> AddCustomerDetailsAsync(CustomerDetailsDto customerDetails)
        {
            int userId = _tokenService.GetUserIdFromToken();
            return await _customerRepo.AddCustomerDetailsFromDbAsync(customerDetails,userId);
        }

        public async Task<ApiResponse<List<CustomerResponseDto>>> GetCustomerDetailsByUserIdAsync()
        {
            int userId = _tokenService.GetUserIdFromToken();
            return await _customerRepo.GetCustomerDetailsByUserIdFromDbAsync(userId);
        }

        public async Task<ApiResponse<CustomerDetails>> DeleteCustomerDetailsAsync(string addressId)
        {
            int userId= _tokenService.GetUserIdFromToken();
            return await _customerRepo.DeleteCustomerDetailsFromDb(addressId,userId);
        }
        public async Task<ApiResponse<CustomerDetails>> UpdateCustomerDetailsAsync(string addressId, CustomerDetailsDto customerDetails)
        {
            int userId= _tokenService.GetUserIdFromToken();
            return await _customerRepo.UpdateCustomerDetailsAsyncFromDb(addressId,customerDetails,userId);
        }
        public async Task<ApiResponse<CustomerDetails>> GetAddressByUserIdAsync(string addressId)
        {
            int userId=_tokenService.GetUserIdFromToken();
            return await _customerRepo.GetAddressByUserIdFromDbAsync(addressId,userId);
        }
    }
}
