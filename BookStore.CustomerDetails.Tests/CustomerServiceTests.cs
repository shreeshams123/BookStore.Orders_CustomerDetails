using BookStore.Orders.Interfaces;
using BookStore.Orders.Model;
using BookStore.Orders.Model.DTOs;
using BookStore.Orders.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.CustomerDetails.Tests
{
    [TestFixture]
    public class CustomerDetailsServiceTests
    {
        private Mock<ICustomerDetailsRepo> _mockCustomerRepo;
        private Mock<TokenService> _mockTokenService;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private Mock<IConfiguration> _mockConfiguration;
        private CustomerDetailsService _customerDetailsService;

        [SetUp]
        public void SetUp()
        {
            _mockCustomerRepo = new Mock<ICustomerDetailsRepo>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            _mockTokenService = new Mock<TokenService>(_mockConfiguration.Object, _mockHttpContextAccessor.Object);

            _customerDetailsService = new CustomerDetailsService(_mockTokenService.Object, _mockCustomerRepo.Object);

            var mockHttpContext = new Mock<HttpContext>();
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            var mockClaim = new Claim(ClaimTypes.NameIdentifier, "123");

            mockClaimsPrincipal.Setup(cp => cp.FindFirst(ClaimTypes.NameIdentifier)).Returns(mockClaim);
            mockHttpContext.Setup(ctx => ctx.User).Returns(mockClaimsPrincipal.Object);
            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext.Object);

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(123);
        }

        [Test]
        public async Task AddCustomerDetailsAsync_Success_ReturnsSuccessMessage()
        {
            var customerDetails = new CustomerDetailsDto
            {
                AddressType = "Home",
                Name = "John Doe",
                Phone = "1234567890",
                Address = "123 Main St",
                City = "Sample City",
                State = "Sample State"
            };

            _mockTokenService.Setup(x => x.GetUserIdFromToken()).Returns(1);
            _mockCustomerRepo.Setup(x => x.AddCustomerDetailsFromDbAsync(customerDetails, 1))
                             .ReturnsAsync(new ApiResponse<CustomerDetailsResponseDto> { Success = true, Message = "Customer details added successfully" });

            var result = await _customerDetailsService.AddCustomerDetailsAsync(customerDetails);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Customer details added successfully", result.Message);
        }

        [Test]
        public async Task GetCustomerDetailsByUserIdAsync_Success_ReturnsCustomerDetails()
        {
            int userId = 1;
            var customerDetails = new List<CustomerDataDto>
            {
                new CustomerDataDto
                {
                    Id = "1",
                    UserId = userId,
                    AddressType = "Home",
                    Name = "John Doe",
                    Phone = "1234567890",
                    Address = "123 Main St",
                    City = "Sample City",
                    State = "Sample State"
                }
            };

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);

            _mockCustomerRepo.Setup(repo => repo.GetCustomerDetailsByUserIdFromDbAsync(userId))
                .ReturnsAsync(new ApiResponse<List<CustomerResponseDto>>
                {
                    Success = true,
                    Message = "Customer details retrieved successfully",
                    Data = new List<CustomerResponseDto>
                    {
                        new CustomerResponseDto { Data = customerDetails }
                    }
                });

            var result = await _customerDetailsService.GetCustomerDetailsByUserIdAsync();

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Customer details retrieved successfully", result.Message);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, result.Data[0].Data.Count);
            Assert.AreEqual("John Doe", result.Data[0].Data[0].Name);
        }

        [Test]
        public async Task DeleteCustomerDetailsAsync_Success_ReturnsSuccessfulResponse()
        {
            string addressId = "address-123";
            int userId = 1;
            var customerDetails = new BookStore.Orders.Model.Entities.CustomerDetails
            {
                Id = ObjectId.GenerateNewId(),
                UserId = userId,
                AddressType = "Home",
                Name = "John Doe",
                Phone = "1234567890",
                Address = "123 Main St",
                City = "Sample City",
                State = "Sample State"
            };

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);

            _mockCustomerRepo.Setup(repo => repo.DeleteCustomerDetailsFromDb(addressId, userId))
                .ReturnsAsync(new ApiResponse<BookStore.Orders.Model.Entities.CustomerDetails>
                {
                    Success = true,
                    Message = "Customer details deleted successfully",
                    Data = customerDetails
                });

            var result = await _customerDetailsService.DeleteCustomerDetailsAsync(addressId);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Customer details deleted successfully", result.Message);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(userId, result.Data.UserId);
            Assert.AreEqual("John Doe", result.Data.Name);
        }

        [Test]
        public async Task DeleteCustomerDetailsAsync_Failure_ReturnsErrorResponse()
        {
            string addressId = "address-123";
            int userId = 1;

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);

            _mockCustomerRepo.Setup(repo => repo.DeleteCustomerDetailsFromDb(addressId, userId))
                .ReturnsAsync(new ApiResponse<BookStore.Orders.Model.Entities.CustomerDetails>
                {
                    Success = false,
                    Message = "Customer details not found for the given address",
                    Data = null
                });

            var result = await _customerDetailsService.DeleteCustomerDetailsAsync(addressId);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Customer details not found for the given address", result.Message);
            Assert.IsNull(result.Data);
        }

        [Test]
        public async Task UpdateCustomerDetailsAsync_Success_ReturnsUpdatedCustomerDetails()
        {
            string addressId = "address-123";
            int userId = 1;

            var customerDetailsDto = new CustomerDetailsDto
            {
                AddressType = "Office",
                Name = "Jane Doe",
                Phone = "9876543210",
                Address = "456 New St",
                City = "New City",
                State = "New State"
            };

            var updatedCustomerDetails = new BookStore.Orders.Model.Entities.CustomerDetails
            {
                Id = ObjectId.GenerateNewId(),
                UserId = userId,
                AddressType = "Office",
                Name = "Jane Doe",
                Phone = "9876543210",
                Address = "456 New St",
                City = "New City",
                State = "New State"
            };

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);

            _mockCustomerRepo.Setup(repo => repo.UpdateCustomerDetailsAsyncFromDb(addressId, customerDetailsDto, userId))
                .ReturnsAsync(new ApiResponse<BookStore.Orders.Model.Entities.CustomerDetails>
                {
                    Success = true,
                    Message = "Customer details updated successfully",
                    Data = updatedCustomerDetails
                });

            var result = await _customerDetailsService.UpdateCustomerDetailsAsync(addressId, customerDetailsDto);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Customer details updated successfully", result.Message);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(updatedCustomerDetails.Name, result.Data.Name);
            Assert.AreEqual(updatedCustomerDetails.Phone, result.Data.Phone);
        }

        [Test]
        public async Task UpdateCustomerDetailsAsync_Failure_ReturnsErrorResponse()
        {
            string addressId = "address-123";
            int userId = 1;

            var customerDetailsDto = new CustomerDetailsDto
            {
                AddressType = "Office",
                Name = "Jane Doe",
                Phone = "9876543210",
                Address = "456 New St",
                City = "New City",
                State = "New State"
            };

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);

            _mockCustomerRepo.Setup(repo => repo.UpdateCustomerDetailsAsyncFromDb(addressId, customerDetailsDto, userId))
                .ReturnsAsync(new ApiResponse<BookStore.Orders.Model.Entities.CustomerDetails>
                {
                    Success = false,
                    Message = "Customer details not found for the given address",
                    Data = null
                });

            var result = await _customerDetailsService.UpdateCustomerDetailsAsync(addressId, customerDetailsDto);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Customer details not found for the given address", result.Message);
            Assert.IsNull(result.Data);
        }

        [Test]
        public async Task GetAddressByUserIdAsync_Success_ReturnsCustomerDetails()
        {
            string addressId = "address-123";
            int userId = 1;

            var expectedCustomerDetails = new BookStore.Orders.Model.Entities.CustomerDetails
            {
                Id = ObjectId.GenerateNewId(),
                UserId = userId,
                AddressType = "Office",
                Name = "Jane Doe",
                Phone = "9876543210",
                Address = "456 New St",
                City = "New City",
                State = "New State"
            };

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);

            _mockCustomerRepo.Setup(repo => repo.GetAddressByUserIdFromDbAsync(addressId, userId))
                .ReturnsAsync(new ApiResponse<BookStore.Orders.Model.Entities.CustomerDetails>
                {
                    Success = true,
                    Message = "Customer details retrieved successfully",
                    Data = expectedCustomerDetails
                });

            var result = await _customerDetailsService.GetAddressByUserIdAsync(addressId);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Customer details retrieved successfully", result.Message);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(expectedCustomerDetails.Name, result.Data.Name);
        }
    }
}
