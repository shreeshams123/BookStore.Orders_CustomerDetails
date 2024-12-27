using BookStore.Orders.Interfaces;
using BookStore.Orders.Model.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Orders.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController:ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToOrders(AddToOrderRequestDto addToOrderRequestDto)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result=await _orderService.AddToOrderAsync(addToOrderRequestDto,token);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrderAsync(string orderId)
        {
            var result = await _orderService.DeleteOrderAsync(orderId);

            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                // Extract token from request headers
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Call the service layer
                var response = await _orderService.GetAllOrdersAsync(token);

                if (!response.Success)
                {
                    return NotFound(new { response.Message });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }

        //}


    }
}
