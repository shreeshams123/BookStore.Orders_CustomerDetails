using BookStore.Orders.Controllers;
using BookStore.Orders.Interfaces;
using BookStore.Orders.Model;
using BookStore.Orders.Model.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.CustomerDetails.Tests
{
    [TestFixture]
    public class CustomerDetailsControllerTests
    {
        private Mock<ICustomerDetailsService> _mockCustomerDetailsService;
        private Mock<HttpContext> _mockHttpContext;
        private CustomerDetailsController _customerDetailsController;

        [SetUp]
        public void SetUp()
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(x => x.Request.Headers["Authorization"]).Returns("Bearer some-valid-token");

            _mockCustomerDetailsService = new Mock<ICustomerDetailsService>();
            _customerDetailsController = new CustomerDetailsController(_mockCustomerDetailsService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }

        [Test]
        public async Task UpdateCustomerDetails_ReturnsOk_WhenUpdateIsSuccessful()
        {
            var addressId = ObjectId.GenerateNewId().ToString();
            var updatedCustomerDetailsDto = new CustomerDetailsDto
            {
                AddressType = "Work",
                Name = "John Doe Updated",
                Phone = "555-9876",
                Address = "456 Elm St",
                City = "Updated City",
                State = "Updated State"
            };

            var updatedCustomerDetails = new BookStore.Orders.Model.Entities.CustomerDetails
            {
                Id = new ObjectId(addressId),
                AddressType = updatedCustomerDetailsDto.AddressType,
                Name = updatedCustomerDetailsDto.Name,
                Phone = updatedCustomerDetailsDto.Phone,
                Address = updatedCustomerDetailsDto.Address,
                City = updatedCustomerDetailsDto.City,
                State = updatedCustomerDetailsDto.State,
                UserId = 1
            };

            var apiResponse = new ApiResponse<BookStore.Orders.Model.Entities.CustomerDetails>
            {
                Success = true,
                Message = "Customer details updated successfully",
                Data = updatedCustomerDetails
            };

            _mockCustomerDetailsService.Setup(service => service.UpdateCustomerDetailsAsync(addressId, updatedCustomerDetailsDto))
                .ReturnsAsync(apiResponse);

            var result = await _customerDetailsController.UpdateCustomerDetails(updatedCustomerDetailsDto, addressId);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(apiResponse, okResult.Value);
        }

        [Test]
        public async Task AddCustomerDetails_ReturnsOk_WhenAddIsSuccessful()
        {
            var customerDetailsDto = new CustomerDetailsDto
            {
                AddressType = "Home",
                Name = "Jane Doe",
                Phone = "555-1234",
                Address = "123 Main St",
                City = "Test City",
                State = "Test State"
            };

            var apiResponse = new ApiResponse<CustomerDetailsResponseDto>
            {
                Success = true,
                Message = "Customer details added successfully",
                Data = new CustomerDetailsResponseDto
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = 1,
                    AddressType = customerDetailsDto.AddressType,
                    Name = customerDetailsDto.Name,
                    Phone = customerDetailsDto.Phone,
                    Address = customerDetailsDto.Address,
                    City = customerDetailsDto.City,
                    State = customerDetailsDto.State
                }
            };

            _mockCustomerDetailsService.Setup(service => service.AddCustomerDetailsAsync(customerDetailsDto))
                .ReturnsAsync(apiResponse);

            var result = await _customerDetailsController.AddCustomerDetails(customerDetailsDto);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(apiResponse, okResult.Value);
        }

        [Test]
        public async Task GetCustomerDetails_ReturnsOk_WhenGetIsSuccessful()
        {
            var customerResponseDto = new CustomerResponseDto
            {
                Data = new List<CustomerDataDto>
                {
                    new CustomerDataDto
                    {
                        Id = "address123",
                        UserId = 1,
                        AddressType = "Home",
                        Name = "Jane Doe",
                        Phone = "555-1234",
                        Address = "123 Main St",
                        City = "Test City",
                        State = "Test State"
                    },
                    new CustomerDataDto
                    {
                        Id = "address124",
                        UserId = 1,
                        AddressType = "Work",
                        Name = "John Doe",
                        Phone = "555-5678",
                        Address = "456 Work St",
                        City = "Test City",
                        State = "Test State"
                    }
                }
            };

            var apiResponse = new ApiResponse<List<CustomerResponseDto>>
            {
                Success = true,
                Message = "Customer details retrieved successfully",
                Data = new List<CustomerResponseDto> { customerResponseDto }
            };

            _mockCustomerDetailsService.Setup(service => service.GetCustomerDetailsByUserIdAsync())
                .ReturnsAsync(apiResponse);

            var result = await _customerDetailsController.GetCustomerDetails();

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(apiResponse, okResult.Value);
        }

        [Test]
        public async Task DeleteCustomerDetails_ReturnsOk_WhenDeleteIsSuccessful()
        {
            var addressId = "address123";
            var customerDetails = new BookStore.Orders.Model.Entities.CustomerDetails
            {
                Id = ObjectId.GenerateNewId(),
                UserId = 1,
                AddressType = "Home",
                Name = "Jane Doe",
                Phone = "555-1234",
                Address = "123 Main St",
                City = "Test City",
                State = "Test State"
            };

            var apiResponse = new ApiResponse<BookStore.Orders.Model.Entities.CustomerDetails>
            {
                Success = true,
                Message = "Customer details deleted successfully",
                Data = customerDetails
            };

            _mockCustomerDetailsService.Setup(service => service.DeleteCustomerDetailsAsync(addressId))
                .ReturnsAsync(apiResponse);

            var result = await _customerDetailsController.DeleteCustomerDetails(addressId);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(apiResponse, okResult.Value);
        }

        [Test]
        public async Task DeleteCustomerDetails_ReturnsBadRequest_WhenDeleteFails()
        {
            var addressId = "address123";
            var apiResponse = new ApiResponse<BookStore.Orders.Model.Entities.CustomerDetails>
            {
                Success = false,
                Message = "Failed to delete customer details",
                Data = null
            };

            _mockCustomerDetailsService.Setup(service => service.DeleteCustomerDetailsAsync(addressId))
                .ReturnsAsync(apiResponse);

            var result = await _customerDetailsController.DeleteCustomerDetails(addressId);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual(apiResponse, badRequestResult.Value);
        }
    }
}
