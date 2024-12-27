using BookStore.Orders.Interfaces;
using BookStore.Orders.Model.DTOs;
using BookStore.Orders.Model.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Orders.Controllers
{
    [ApiController]
    [Route("api/customerDetails")]
    public class CustomerDetailsController : ControllerBase
    {
        private readonly ICustomerDetailsService _customerDetailsService;
        public CustomerDetailsController(ICustomerDetailsService customerDetailsService)
        {
            _customerDetailsService = customerDetailsService;
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomerDetails([FromBody] CustomerDetailsDto customerDetails)
        {
            var addedCustomer = await _customerDetailsService.AddCustomerDetailsAsync(customerDetails);
            if (addedCustomer.Success)
            {
                return Ok(addedCustomer);
            }
            return BadRequest(addedCustomer);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerDetails()
        {
            var customerDetails = await _customerDetailsService.GetCustomerDetailsByUserIdAsync();
            if (customerDetails.Success)
            {
                return Ok(customerDetails);
            }
            return BadRequest(customerDetails);
        }
        [HttpDelete("{addressId}")]
        public async Task<IActionResult> DeleteCustomerDetails(string addressId)
        {
           var customer=await _customerDetailsService.DeleteCustomerDetailsAsync(addressId);
            if (customer.Success)
            {
                return Ok(customer);
            }
            else
            {
                return BadRequest(customer);
            }
        }
        [HttpPatch("{addressId}")]
        public async Task<IActionResult> UpdateCustomerDetails(CustomerDetailsDto customerDetails,string addressId)
        {
            var customer=await _customerDetailsService.UpdateCustomerDetailsAsync(addressId, customerDetails);
            if (customer.Success)
            {
                return Ok(customer);
            }
            else
            {
                return BadRequest(customer);
            }
        }
        [HttpGet("{addressId}")]
        public async Task<IActionResult> GetAddressByUserId(string addressId)
        {
            var customer=await _customerDetailsService.GetAddressByUserIdAsync(addressId);
            if (customer.Success)
            {
                return Ok(customer);
            }
            else
            {
                return BadRequest(customer);
            }
        }
    }
}
