using Microsoft.AspNetCore.Mvc;
using ServiceConnect.BLL.Factories;
using ServiceConnect.Models.DTOs;

namespace ServiceConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    private readonly ServiceFactory _factory;

    public BidsController(ServiceFactory factory)
    {
        _factory = factory;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BidDTO>>> GetAllBids()
    {
        try
        {
            var service = _factory.CreateBidService();
            var bids = await service.GetAllBidsAsync();
            return Ok(bids);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BidDTO>> GetBid(int id)
    {
        try
        {
            var service = _factory.CreateBidService();
            var bid = await service.GetBidByIdAsync(id);

            if (bid == null)
                return NotFound(new { error = $"Bid with ID {id} not found" });

            return Ok(bid);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("job/{jobId}")]
    public async Task<ActionResult<IEnumerable<BidDTO>>> GetBidsByJob(int jobId)
    {
        try
        {
            var service = _factory.CreateBidService();
            var bids = await service.GetBidsByJobAsync(jobId);
            return Ok(bids);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("worker/{workerId}")]
    public async Task<ActionResult<IEnumerable<BidDTO>>> GetBidsByWorker(int workerId)
    {
        try
        {
            var service = _factory.CreateBidService();
            var bids = await service.GetBidsByWorkerAsync(workerId);
            return Ok(bids);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("job/{jobId}/stats")]
    public async Task<ActionResult> GetBidStats(int jobId)
    {
        try
        {
            var service = _factory.CreateBidService();
            var stats = await service.GetBidStatsAsync(jobId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<BidDTO>> CreateBid([FromBody] BidDTO bidDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = _factory.CreateBidService();
            var createdBid = await service.CreateBidAsync(bidDto);
            return CreatedAtAction(nameof(GetBid), new { id = createdBid.BidID }, createdBid);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/accept")]
    public async Task<ActionResult> AcceptBid(int id)
    {
        try
        {
            var service = _factory.CreateBidService();
            var bookingCode = await service.AcceptBidAsync(id);
            return Ok(new { message = "Bid accepted successfully", bookingCode, bidId = id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateBid(int id, [FromBody] BidDTO bidDto)
    {
        try
        {
            if (bidDto.BidID != id)
                return BadRequest(new { error = "Bid ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = _factory.CreateBidService();
            var success = await service.UpdateBidAsync(bidDto);

            if (!success)
                return NotFound(new { error = $"Bid with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBid(int id)
    {
        try
        {
            var service = _factory.CreateBidService();
            var success = await service.DeleteBidAsync(id);

            if (!success)
                return NotFound(new { error = $"Bid with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
