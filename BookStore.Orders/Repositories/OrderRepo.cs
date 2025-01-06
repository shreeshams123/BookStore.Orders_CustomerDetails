using BookStore.Orders.Data;
using BookStore.Orders.Interfaces;
using BookStore.Orders.Model;
using BookStore.Orders.Model.DTOs;
using BookStore.Orders.Model.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookStore.Orders.Repositories
{
    public class OrderRepo:IOrderRepo
    {
        private readonly IMongoCollection<Order> _orders;

        public OrderRepo(MongoDbService mongoDbService)
        {
            _orders = mongoDbService.Database.GetCollection<Order>("orders");
        }

        public async Task<ApiResponse<Order>> AddOrderAsync(Order order)
        {
            await _orders.InsertOneAsync(order);
            return new ApiResponse<Order>
            {
                Success = true,
                Message = "Added Order Successfully",
                Data = order
            };
        }
        public async Task<ApiResponse<Order>> GetOrderByIdAsync(string orderId, int userId)
        {
            try
            {
                var objectId = new ObjectId(orderId);
                var filter = Builders<Order>.Filter.And(
                    Builders<Order>.Filter.Eq(o => o.Id, objectId),
                    Builders<Order>.Filter.Eq(o => o.UserId, userId)
                );

                var order = await _orders.Find(filter).FirstOrDefaultAsync();

                if (order == null)
                {
                    return new ApiResponse<Order>
                    {
                        Success = false,
                        Message = "Order not found."
                    };
                }

                return new ApiResponse<Order>
                {
                    Success = true,
                    Message = "Order retrieved successfully.",
                    Data = order
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Order>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving the order: {ex.Message}"
                };
            }
        }
        public async Task<ApiResponse<Order>> DeleteOrderFromDbAsync(string orderId, int userId)
        {
            try
            {
                var filter = Builders<Order>.Filter.And(
                    Builders<Order>.Filter.Eq(o => o.Id, ObjectId.Parse(orderId)),
                    Builders<Order>.Filter.Eq(o => o.UserId, userId)
                );

                var order = await _orders.Find(filter).FirstOrDefaultAsync();

                if (order == null)
                {
                    return new ApiResponse<Order>
                    {
                        Success = false,
                        Message = "Order not found."
                    };
                }

                var result = await _orders.DeleteOneAsync(filter);

                if (result.DeletedCount == 0)
                {
                    return new ApiResponse<Order>
                    {
                        Success = false,
                        Message = "Failed to delete the order."
                    };
                }

                return new ApiResponse<Order>
                {
                    Success = true,
                    Message = "Order deleted successfully.",
                    Data = order
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Order>
                {
                    Success = false,
                    Message = $"An error occurred while deleting the order: {ex.Message}"
                };
            }
        }
        public async Task<ApiResponse<List<Order>>> GetAllOrdersFromDbAsync(int userId)
        {
            try
            {
                var filter = Builders<Order>.Filter.Eq(o => o.UserId, userId);
                var orders = await _orders.Find(filter).ToListAsync();

                if (orders == null || orders.Count == 0)
                {
                    return new ApiResponse<List<Order>>
                    {
                        Success = true,
                        Message = "No orders found.",
                        Data= new List<Order>()
                    };
                }

                return new ApiResponse<List<Order>>
                {
                    Success = true,
                    Message = "Orders retrieved successfully.",
                    Data = orders
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<Order>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving orders: {ex.Message}"
                };
            }
        }



    }
}
