using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BookStore.Orders.Model.Entities
{
    public class Order
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int UserId { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
        public int TotalQuantity {  get; set; }
        public DateTime Date {  get; set; }
        public decimal TotalPrice { get; set; }
        public string AddressId { get; set; }
        public string Status { get; set; }

    }
    public class OrderItemDto
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
    }
}
