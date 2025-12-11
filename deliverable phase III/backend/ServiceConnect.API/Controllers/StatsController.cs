using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceConnect.BLL.Data;

namespace ServiceConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly ServiceConnectDbContext _context;

    public StatsController(ServiceConnectDbContext context)
    {
        _context = context;
    }

    [HttpGet("counts")]
    public async Task<IActionResult> GetCounts()
    {
        var counts = new
        {
            Users = await _context.Users.CountAsync(),
            Workers = await _context.Workers.CountAsync(),
            Customers = await _context.Customers.CountAsync(),
            ServiceCategories = await _context.ServiceCategories.CountAsync(),
            Jobs = await _context.Jobs.CountAsync(),
            Bids = await _context.Bids.CountAsync(),
            Bookings = await _context.Bookings.CountAsync(),
            Reviews = await _context.Reviews.CountAsync(),
            Notifications = await _context.Notifications.CountAsync()
        };

        return Ok(counts);
    }
}
