using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BookStore.Orders.Model.Entities
{
    public class CustomerDetails
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int UserId { get; set; }
        public string AddressType { get; set; } 
        public string Name { get; set; } 
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }
}
