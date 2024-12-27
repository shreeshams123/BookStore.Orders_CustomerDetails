using BookStore.Orders.Model.Entities;

namespace BookStore.Orders.Model.DTOs
{
    public class OrderDataDto
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal BookPrice { get; set; }
        public string Author { get; set; }
        public string Image { get; set; } 
    }


    public class OrderResponseDto
    {
        public string Id { get; set; }
        public int UserId { get; set; }
        public List<OrderDataDto> OrderItems { get; set; }
        public int TotalQuantity { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalPrice { get; set; }
        public string AddressId { get; set; }
        public string Status { get; set; }
    }

}
