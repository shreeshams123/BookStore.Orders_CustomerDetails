using BookStore.Orders.Data;
using BookStore.Orders.Interfaces;
using BookStore.Orders.Model;
using BookStore.Orders.Model.DTOs;
using BookStore.Orders.Model.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookStore.Orders.Repositories
{
    public class CustomerDetailsRepo : ICustomerDetailsRepo
    {
        private readonly IMongoCollection<CustomerDetails> _customerDetailsCollection;

        public CustomerDetailsRepo(MongoDbService mongoDbService)
        {
            _customerDetailsCollection = mongoDbService.Database.GetCollection<CustomerDetails>("CustomerDetails");
        }

        public async Task<ApiResponse<CustomerDetailsResponseDto>> AddCustomerDetailsFromDbAsync(CustomerDetailsDto customerDetails, int userId)
        {
            var newCustomer = new CustomerDetails
            {

                UserId = userId,
                Name = customerDetails.Name,
                Address = customerDetails.Address,
                AddressType = customerDetails.AddressType,
                Phone = customerDetails.Phone,
                City = customerDetails.City,
                State = customerDetails.State,
            };
            await _customerDetailsCollection.InsertOneAsync(newCustomer);
            return new ApiResponse<CustomerDetailsResponseDto>
            {
                Success = true,
                Message = "Added customer details successfully",
                Data = new CustomerDetailsResponseDto
                {
                    Id = newCustomer.Id.ToString(),
                    UserId = newCustomer.UserId,
                    Name = newCustomer.Name,
                    Address = newCustomer.Address,
                    AddressType = newCustomer.AddressType,
                    Phone = newCustomer.Phone,
                    City = newCustomer.City,
                    State = newCustomer.State
                }
            };
        }

        public async Task<ApiResponse<List<CustomerResponseDto>>> GetCustomerDetailsByUserIdFromDbAsync(int userId)
        {
            var customerDetailsList = await _customerDetailsCollection
                .Find(c => c.UserId == userId)
                .ToListAsync();

            if (customerDetailsList == null || !customerDetailsList.Any())
            {
                return new ApiResponse<List<CustomerResponseDto>>
                {
                    Success = false,
                    Message = "No customer details found for the given user.",
                    Data = null
                };
            }
            var customerDtos = customerDetailsList.Select(c => new CustomerDataDto
            {
                Id = c.Id.ToString(),
                UserId = c.UserId,
                Address = c.Address,
                Name = c.Name,
                AddressType = c.AddressType,
                Phone = c.Phone,
                City = c.City,
                State = c.State,
            }).ToList();
            var responseDto = new CustomerResponseDto
            {
                Data = customerDtos,
            };
            return new ApiResponse<List<CustomerResponseDto>>
            {
                Success = true,
                Message = "Customer details fetched successfully.",
                Data = new List<CustomerResponseDto> { responseDto }
            };
        }
        public async Task<ApiResponse<CustomerDetails>> DeleteCustomerDetailsFromDb(string addressId, int userId)
        {
            var objectId = ObjectId.Parse(addressId);
            var filter = Builders<CustomerDetails>.Filter.And(
                Builders<CustomerDetails>.Filter.Eq(o => o.Id, objectId),
                Builders<CustomerDetails>.Filter.Eq(o => o.UserId, userId)
            );
            var customer = await _customerDetailsCollection.Find(filter).FirstOrDefaultAsync();

            if (customer == null)
            {
                return new ApiResponse<CustomerDetails>
                {
                    Success = false,
                    Message = "Address not found"
                };
            }

            var deleteResult = await _customerDetailsCollection.DeleteOneAsync(filter);

            if (deleteResult.DeletedCount > 0)
            {
                return new ApiResponse<CustomerDetails>
                {
                    Success = true,
                    Message = "Deleted address successfully."
                };
            }

            return new ApiResponse<CustomerDetails>
            {
                Success = false,
                Message = "Failed to delete the address."
            };
        }

        public async Task<ApiResponse<CustomerDetails>> UpdateCustomerDetailsAsyncFromDb(string addressId, CustomerDetailsDto updatedCustomerDetails, int userId)
        {
            var objectId = ObjectId.Parse(addressId);
            var filter = Builders<CustomerDetails>.Filter.And(
                Builders<CustomerDetails>.Filter.Eq(o => o.Id, objectId),
                Builders<CustomerDetails>.Filter.Eq(o => o.UserId, userId)
            );
            var existingCustomer = await _customerDetailsCollection.Find(filter).FirstOrDefaultAsync();

            if (existingCustomer == null)
            {
                return new ApiResponse<CustomerDetails>
                {
                    Success = false,
                    Message = "Customer details not found."
                };
            }
            if (!string.IsNullOrEmpty(updatedCustomerDetails.Name))
            {
                existingCustomer.Name = updatedCustomerDetails.Name;
            }

            if (!string.IsNullOrEmpty(updatedCustomerDetails.Address))
            {
                existingCustomer.Address = updatedCustomerDetails.Address;
            }

            if (!string.IsNullOrEmpty(updatedCustomerDetails.AddressType))
            {
                existingCustomer.AddressType = updatedCustomerDetails.AddressType;
            }

            if (!string.IsNullOrEmpty(updatedCustomerDetails.Phone))
            {
                existingCustomer.Phone = updatedCustomerDetails.Phone;
            }

            if (!string.IsNullOrEmpty(updatedCustomerDetails.City))
            {
                existingCustomer.City = updatedCustomerDetails.City;
            }

            if (!string.IsNullOrEmpty(updatedCustomerDetails.State))
            {
                existingCustomer.State = updatedCustomerDetails.State;
            }

            var updateResult = await _customerDetailsCollection.ReplaceOneAsync(filter, existingCustomer);

            if (updateResult.ModifiedCount > 0)
            {
                return new ApiResponse<CustomerDetails>
                {
                    Success = true,
                    Message = "Customer details updated successfully.",
                    Data = existingCustomer
                };
            }

            return new ApiResponse<CustomerDetails>
            {
                Success = false,
                Message = "No changes were made to the customer details."
            };
        }
        public async Task<ApiResponse<CustomerDetails>> GetAddressByUserIdFromDbAsync(string addressId, int userId)
        {
            var objectId = ObjectId.Parse(addressId);

            var filter = Builders<CustomerDetails>.Filter.And(
                Builders<CustomerDetails>.Filter.Eq(o => o.Id, objectId),
                Builders<CustomerDetails>.Filter.Eq(o => o.UserId, userId)
            );

            var customerDetails = await _customerDetailsCollection.Find(filter).FirstOrDefaultAsync();

            if (customerDetails == null)
            {
                return new ApiResponse<CustomerDetails>
                {
                    Success = false,
                    Message = "Customer details not found."
                };
            }

            return new ApiResponse<CustomerDetails>
            {
                Success = true,
                Message = "Customer details retrieved successfully.",
                Data = customerDetails
            };
        }
    }
}