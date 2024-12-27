using BookStore.Orders.Model;
using BookStore.Orders.Model.DTOs;

namespace BookStore.Orders.Interfaces
{
    public interface IExternalService
    {
        Task<BookDataDto> BookExists(int bookId);
        Task<CartDataDtoList> GetItemsFromCart(string token);
        Task<ClearCartResponse> DeleteFromCart(int userId, int bookId,string token);
        Task<ApiResponse<BookDataDto>> UpdateBookStockAsync(int bookId, int quantity, string token);
    }
}
