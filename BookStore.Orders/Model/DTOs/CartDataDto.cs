namespace BookStore.Orders.Model.DTOs
{
    public class CartDataDto
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string Description { get; set; }
        public string BookImage { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CartDataDtoList
    {
        public List<CartDataDto> CartItems { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CartResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public CartDataDtoList Data { get; set; } // Updated to match the JSON structure
    }

    public class ClearCartResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}