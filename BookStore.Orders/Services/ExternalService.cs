using BookStore.Orders.Interfaces;
using BookStore.Orders.Model;
using BookStore.Orders.Model.DTOs;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BookStore.Orders.Services
{
    public class ExternalService:IExternalService
    {
        private readonly HttpClient _httpClient;
        private readonly string _bookServiceUrl = "https://localhost:7183/api/book/";
        private readonly string _cartServiceUrl = "https://localhost:7283/api/cart";
        
        public ExternalService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<BookDataDto> BookExists(int bookId)
        {
            Console.WriteLine($"Checking if book with ID {bookId} exists...");

            var response = await _httpClient.GetAsync($"{_bookServiceUrl}{bookId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Received response: {content}");

                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var bookResponse = JsonSerializer.Deserialize<BookResponseDto>(content, options);
                    Console.WriteLine($"Deserialized bookResponse: {JsonSerializer.Serialize(bookResponse)}");

                    if (bookResponse != null && bookResponse.Success)
                    {
                        Console.WriteLine($"Book found: {bookResponse.Data.Title}");
                        return bookResponse.Data;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to find book with ID {bookId}. Response was: {content}");
                        return null;
                    }
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"Error deserializing response: {jsonEx.Message}");
                    return null;
                }
            }
            else
            {
                Console.WriteLine($"Failed to fetch book data. Status Code: {response.StatusCode}");
                return null;
            }
        }
        public async Task<CartDataDtoList> GetItemsFromCart(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("No token provided.");
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(_cartServiceUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Received response: {content}");

                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    // Deserialize the response into CartResponseDto
                    var cartResponse = JsonSerializer.Deserialize<CartResponseDto>(content, options);
                    Console.WriteLine($"Deserialized cartResponse: {JsonSerializer.Serialize(cartResponse)}");

                    if (cartResponse != null && cartResponse.Success)
                    {
                        Console.WriteLine($"Cart items retrieved successfully.");
                        return cartResponse.Data; // Return the CartDataDtoList
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve cart items. Response was: {content}");
                        return null;
                    }
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"Error deserializing response: {jsonEx.Message}");
                    return null;
                }
            }
            else
            {
                Console.WriteLine($"Failed to fetch cart data. Status Code: {response.StatusCode}");
                return null;
            }
        }

        public async Task<ClearCartResponse> DeleteFromCart(int userId, int bookId,string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("No token provided.");
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestUrl = $"{_cartServiceUrl}/{bookId}";

            var response = await _httpClient.DeleteAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Received response: {content}");

                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var deleteResponse = JsonSerializer.Deserialize<ClearCartResponse>(content, options);
                    Console.WriteLine($"Deserialized deleteResponse: {JsonSerializer.Serialize(deleteResponse)}");

                    if (deleteResponse != null && deleteResponse.Success)
                    {
                        Console.WriteLine($"Successfully removed item with ID {bookId} from cart.");
                        return deleteResponse;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to remove item with ID {bookId}. Response was: {content}");
                        return new ClearCartResponse { Success = false, Message = $"Failed to remove item with ID {bookId}" };
                    }
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"Error deserializing response: {jsonEx.Message}");
                    return new ClearCartResponse { Success = false, Message = "Error deserializing response" };
                }
            }
            else
            {
                Console.WriteLine($"Failed to delete item from cart. Status Code: {response.StatusCode}");
                return new ClearCartResponse { Success = false, Message = $"Failed to delete item. Status Code: {response.StatusCode}" };
            }
        }
        public async Task<ApiResponse<BookDataDto>> UpdateBookStockAsync(int bookId, int quantity, string token)
        {
            var updateDto = new UpdateBookDto
            {
                StockQuantity = quantity
            };

            var jsonContent = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PutAsync($"{_bookServiceUrl}{bookId}", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                // Configure JSON deserialization options
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var bookResponse = JsonSerializer.Deserialize<ApiResponse<BookDataDto>>(responseContent, options);

                if (bookResponse != null)
                {
                    Console.WriteLine($"Deserialized bookResponse: {JsonSerializer.Serialize(bookResponse)}");
                    if (bookResponse.Success)
                    {
                        return bookResponse;
                    }
                }

                Console.WriteLine($"Failed to update stock for book ID {bookId}. Response was: {responseContent}");
                return new ApiResponse<BookDataDto> { Success = false, Message = "Failed to update stock." };
            }
            else
            {
                Console.WriteLine($"Failed to update stock for book ID {bookId}. Status Code: {response.StatusCode}");
                return new ApiResponse<BookDataDto> { Success = false, Message = "HTTP request failed." };
            }
        }



    }

}
