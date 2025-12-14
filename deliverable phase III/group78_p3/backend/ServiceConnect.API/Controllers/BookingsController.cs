using Microsoft.AspNetCore.Mvc;
using ServiceConnect.BLL.Factories;
using ServiceConnect.Models.DTOs;

namespace ServiceConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly ServiceFactory _factory;

    public BookingsController(ServiceFactory factory)
    {
        _factory = factory;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingDTO>>> GetAllBookings()
    {
        try
        {
            var service = _factory.CreateBookingService();
            var bookings = await service.GetAllBookingsAsync();
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookingDTO>> GetBooking(int id)
    {
        try
        {
            var service = _factory.CreateBookingService();
            var booking = await service.GetBookingByIdAsync(id);

            if (booking == null)
                return NotFound(new { error = $"Booking with ID {id} not found" });

            return Ok(booking);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("worker/{workerId}")]
    public async Task<ActionResult<IEnumerable<BookingDTO>>> GetBookingsByWorker(int workerId)
    {
        try
        {
            var service = _factory.CreateBookingService();
            var bookings = await service.GetBookingsByWorkerAsync(workerId);
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<BookingDTO>>> GetBookingsByCustomer(int customerId)
    {
        try
        {
            var service = _factory.CreateBookingService();
            var bookings = await service.GetBookingsByCustomerAsync(customerId);
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("summary-by-category")]
    public async Task<ActionResult> GetSummaryByCategory()
    {
        try
        {
            var service = _factory.CreateBookingService();
            var summary = await service.GetBookingSummaryByCategoryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<BookingDTO>> CreateBooking([FromBody] BookingDTO bookingDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = _factory.CreateBookingService();
            var createdBooking = await service.CreateBookingAsync(bookingDto);
            return CreatedAtAction(nameof(GetBooking), new { id = createdBooking.BookingID }, createdBooking);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult> CompleteBooking(int id, [FromBody] string? completionNotes)
    {
        try
        {
            var service = _factory.CreateBookingService();
            var success = await service.CompleteBookingAsync(id, completionNotes);

            if (!success)
                return NotFound(new { error = $"Booking with ID {id} not found" });

            return Ok(new { message = "Booking completed successfully", bookingId = id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateBooking(int id, [FromBody] BookingDTO bookingDto)
    {
        try
        {
            if (bookingDto.BookingID != id)
                return BadRequest(new { error = "Booking ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = _factory.CreateBookingService();
            var success = await service.UpdateBookingAsync(bookingDto);

            if (!success)
                return NotFound(new { error = $"Booking with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBooking(int id)
    {
        try
        {
            var service = _factory.CreateBookingService();
            var success = await service.DeleteBookingAsync(id);

            if (!success)
                return NotFound(new { error = $"Booking with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex) when (ex is InvalidOperationException)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
