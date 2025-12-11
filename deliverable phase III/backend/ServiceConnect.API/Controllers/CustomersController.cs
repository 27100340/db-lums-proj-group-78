using Microsoft.AspNetCore.Mvc;
using ServiceConnect.BLL.Factories;
using ServiceConnect.Models.DTOs;

namespace ServiceConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ServiceFactory _factory;

    public CustomersController(ServiceFactory factory)
    {
        _factory = factory;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetAllCustomers()
    {
        try
        {
            var service = _factory.CreateCustomerService();
            var customers = await service.GetAllCustomersAsync();
            return Ok(customers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDTO>> GetCustomer(int id)
    {
        try
        {
            var service = _factory.CreateCustomerService();
            var customer = await service.GetCustomerByIdAsync(id);

            if (customer == null)
                return NotFound(new { error = $"Customer with ID {id} not found" });

            return Ok(customer);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("city/{city}")]
    public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomersByCity(string city)
    {
        try
        {
            var service = _factory.CreateCustomerService();
            var customers = await service.GetCustomersByCityAsync(city);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("analytics")]
    public async Task<ActionResult> GetCustomerAnalytics()
    {
        try
        {
            var service = _factory.CreateCustomerService();
            var analytics = await service.GetCustomerAnalyticsAsync();
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDTO>> CreateCustomer([FromBody] CustomerDTO customerDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = _factory.CreateCustomerService();
            var createdCustomer = await service.CreateCustomerAsync(customerDto);
            return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.CustomerID }, createdCustomer);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCustomer(int id, [FromBody] CustomerDTO customerDto)
    {
        try
        {
            if (customerDto.CustomerID != id)
                return BadRequest(new { error = "Customer ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = _factory.CreateCustomerService();
            var success = await service.UpdateCustomerAsync(customerDto);

            if (!success)
                return NotFound(new { error = $"Customer with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCustomer(int id)
    {
        try
        {
            var service = _factory.CreateCustomerService();
            var success = await service.DeleteCustomerAsync(id);

            if (!success)
                return NotFound(new { error = $"Customer with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
