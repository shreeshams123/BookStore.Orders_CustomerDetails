using BookStore.Orders.Data;
using BookStore.Orders.Model.DTOs;
using BookStore.Orders.Repositories;
using Microsoft.Extensions.Configuration;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using Newtonsoft.Json;

namespace BookStore.CustomerDetails.Tests
{
    [TestFixture]
    public class CustomerRepoTests:IDisposable
    {
        private MongoDbService _mockMongoDbService;
        private CustomerDetailsRepo _customerDetailsRepo;
        private MongoDbRunner _mongoDbRunner;
        private IMongoCollection<BookStore.Orders.Model.Entities.CustomerDetails> _mockCollection;
        private string _connectionString;

        [SetUp]
        public void SetUp()
        {
            _mongoDbRunner = MongoDbRunner.Start();
            _connectionString = $"{_mongoDbRunner.ConnectionString}TestDatabase";

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["ConnectionStrings:DbConnection"]).Returns(_connectionString);

            _mockMongoDbService = new MongoDbService(configurationMock.Object);

            _mockCollection = _mockMongoDbService.Database.GetCollection<BookStore.Orders.Model.Entities.CustomerDetails>("customerDetails");

            _customerDetailsRepo = new CustomerDetailsRepo(_mockMongoDbService);
        }

        [Test]
        public async Task AddCustomerDetailsFromDbAsync_ShouldAddCustomerDetails_WhenValidData()
        {
            var customerDetailsDto = new CustomerDetailsDto
            {
                Name = "John Doe",
                Address = "123 Main St",
                AddressType = "Home",
                Phone = "1234567890",
                City = "New York",
                State = "NY"
            };
            var userId = 1;

            var response = await _customerDetailsRepo.AddCustomerDetailsFromDbAsync(customerDetailsDto, userId);

            Assert.IsTrue(response.Success);
            Assert.AreEqual("Added customer details successfully", response.Message);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(customerDetailsDto.Name, response.Data.Name);
            Assert.AreEqual(customerDetailsDto.Address, response.Data.Address);
            Assert.AreEqual(customerDetailsDto.Phone, response.Data.Phone);
        }
       






        [TearDown]
        public void TearDown()
        {
            _mongoDbRunner?.Dispose();
        }

        public void Dispose()
        {
            TearDown();
        }
    }
}