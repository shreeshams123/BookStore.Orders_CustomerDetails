using BookStore.Orders.Data;
using BookStore.Orders.Model.Entities;
using BookStore.Orders.Repositories;
using Microsoft.Extensions.Configuration;
using Mongo2Go;
using MongoDB.Driver;
using Moq;
using MongoDB.Bson;


namespace BookStore.Orders.Tests
{
    [TestFixture]
    public class OrderRepoTests : IDisposable
    {
        private MongoDbService _mockMongoDbService;
        private OrderRepo _orderRepo;
        private MongoDbRunner _mongoDbRunner;
        private IMongoCollection<Order> _mockCollection;
        private string _connectionString;

        [SetUp]
        public void SetUp()
        {
            _mongoDbRunner = MongoDbRunner.Start();
            _connectionString = $"{_mongoDbRunner.ConnectionString}TestDatabase";
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["ConnectionStrings:DbConnection"]).Returns(_connectionString);
            _mockMongoDbService = new MongoDbService(configurationMock.Object);
            _mockCollection = _mockMongoDbService.Database.GetCollection<Order>("orders");
            _orderRepo = new OrderRepo(_mockMongoDbService);
        }

        [Test]
        public async Task AddOrderAsync_ShouldInsertNewOrder_WhenValidOrder()
        {
            var order = new Order
            {
                UserId = 1,
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto { BookId = 101, Quantity = 2 }
                },
                TotalPrice = 50,
                TotalQuantity = 2,
                Date = DateTime.Now,
                Status = "Pending",
                AddressId = "12345"
            };

            var response = await _orderRepo.AddOrderAsync(order);

            Assert.IsTrue(response.Success);
            Assert.AreEqual("Added Order Successfully", response.Message);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(1, response.Data.UserId);
            Assert.AreEqual(1, response.Data.OrderItems.Count);
            Assert.AreEqual(101, response.Data.OrderItems[0].BookId);
            Assert.AreEqual(2, response.Data.OrderItems[0].Quantity);
        }

        [Test]
        public async Task GetOrderByIdAsync_ShouldReturnOrder_WhenOrderExists()
        {
            var order = new Order
            {
                Id = ObjectId.GenerateNewId(),
                UserId = 1,
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto { BookId = 101, Quantity = 2 }
                },
                TotalPrice = 50,
                TotalQuantity = 2,
                Date = DateTime.Now,
                Status = "Pending",
                AddressId = "12345"
            };

            await _mockCollection.InsertOneAsync(order);

            var response = await _orderRepo.GetOrderByIdAsync(order.Id.ToString(), 1);

            Assert.IsTrue(response.Success);
            Assert.AreEqual("Order retrieved successfully.", response.Message);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(order.Id, response.Data.Id);
        }

        [Test]
        public async Task GetOrderByIdAsync_ShouldReturnError_WhenOrderDoesNotExist()
        {
            var response = await _orderRepo.GetOrderByIdAsync(ObjectId.GenerateNewId().ToString(), 1);

            Assert.IsFalse(response.Success);
            Assert.AreEqual("Order not found.", response.Message);
        }

        [Test]
        public async Task DeleteOrderFromDbAsync_ShouldDeleteOrder_WhenOrderExists()
        {
            var order = new Order
            {
                Id = ObjectId.GenerateNewId(),
                UserId = 1,
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto { BookId = 101, Quantity = 2 }
                },
                TotalPrice = 50,
                TotalQuantity = 2,
                Date = DateTime.Now,
                Status = "Pending",
                AddressId = "12345"
            };

            await _mockCollection.InsertOneAsync(order);

            var response = await _orderRepo.DeleteOrderFromDbAsync(order.Id.ToString(), 1);

            Assert.IsTrue(response.Success);
            Assert.AreEqual("Order deleted successfully.", response.Message);

            var deletedOrder = await _mockCollection.Find(o => o.Id == order.Id).FirstOrDefaultAsync();
            Assert.IsNull(deletedOrder);
        }

        [Test]
        public async Task DeleteOrderFromDbAsync_ShouldReturnError_WhenOrderDoesNotExist()
        {
            var response = await _orderRepo.DeleteOrderFromDbAsync(ObjectId.GenerateNewId().ToString(), 1);

            Assert.IsFalse(response.Success);
            Assert.AreEqual("Order not found.", response.Message);
        }

        [Test]
        public async Task GetAllOrdersFromDbAsync_ShouldReturnAllOrders_WhenOrdersExist()
        {
            var order = new Order
            {
                Id = ObjectId.GenerateNewId(),
                UserId = 1,
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto { BookId = 101, Quantity = 2 }
                },
                TotalPrice = 50,
                TotalQuantity = 2,
                Date = DateTime.Now,
                Status = "Pending",
                AddressId = "12345"
            };

            await _mockCollection.InsertOneAsync(order);

            var response = await _orderRepo.GetAllOrdersFromDbAsync(1);

            Assert.IsTrue(response.Success);
            Assert.AreEqual("Orders retrieved successfully.", response.Message);
            Assert.AreEqual(1, response.Data.Count);
            Assert.AreEqual(1, response.Data[0].UserId);
        }

        [Test]
        public async Task GetAllOrdersFromDbAsync_ShouldReturnEmpty_WhenNoOrdersExist()
        {
            var response = await _orderRepo.GetAllOrdersFromDbAsync(999);

            Assert.IsTrue(response.Success);
            Assert.AreEqual("No orders found.", response.Message);
            Assert.AreEqual(0, response.Data.Count);
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