using Microsoft.AspNetCore.Mvc;
using ServiceConnect.BLL.Factories;
using ServiceConnect.Models.DTOs;

namespace ServiceConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly ServiceFactory _factory;

    public JobsController(ServiceFactory factory)
    {
        _factory = factory;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobDTO>>> GetAllJobs()
    {
        try
        {
            var service = _factory.CreateJobService();
            var jobs = await service.GetAllJobsAsync();
            return Ok(jobs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobDTO>> GetJob(int id)
    {
        try
        {
            var service = _factory.CreateJobService();
            var job = await service.GetJobByIdAsync(id);

            if (job == null)
                return NotFound(new { error = $"Job with ID {id} not found" });

            return Ok(job);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("open")]
    public async Task<ActionResult<IEnumerable<JobDTO>>> GetOpenJobs()
    {
        try
        {
            var service = _factory.CreateJobService();
            var jobs = await service.GetOpenJobsAsync();
            return Ok(jobs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<JobDTO>>> GetJobsByCategory(int categoryId)
    {
        try
        {
            var service = _factory.CreateJobService();
            var jobs = await service.GetJobsByCategoryAsync(categoryId);
            return Ok(jobs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<JobDTO>>> GetJobsByCustomer(int customerId)
    {
        try
        {
            var service = _factory.CreateJobService();
            var jobs = await service.GetJobsByCustomerAsync(customerId);
            return Ok(jobs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("location/{city}/category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<JobDTO>>> GetJobsByLocation(string city, int categoryId)
    {
        try
        {
            var service = _factory.CreateJobService();
            var jobs = await service.GetJobsByLocationAsync(city, categoryId);
            return Ok(jobs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("active-with-bids")]
    public async Task<ActionResult> GetActiveJobsWithBids()
    {
        try
        {
            var service = _factory.CreateJobService();
            var jobs = await service.GetActiveJobsWithBidsAsync();
            return Ok(jobs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{jobId}/complexity")]
    public async Task<ActionResult<int>> CalculateComplexity(int jobId, [FromQuery] decimal budget, [FromQuery] string urgencyLevel, [FromQuery] int requiredWorkers)
    {
        try
        {
            var service = _factory.CreateJobService();
            var complexity = await service.CalculateJobComplexityAsync(budget, urgencyLevel, requiredWorkers);
            return Ok(new { jobId, complexity });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<JobDTO>> CreateJob([FromBody] JobDTO jobDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = _factory.CreateJobService();
            var createdJob = await service.CreateJobAsync(jobDto);
            return CreatedAtAction(nameof(GetJob), new { id = createdJob.JobID }, createdJob);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateJob(int id, [FromBody] JobDTO jobDto)
    {
        try
        {
            if (jobDto.JobID != id)
                return BadRequest(new { error = "Job ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = _factory.CreateJobService();
            var success = await service.UpdateJobAsync(jobDto);

            if (!success)
                return NotFound(new { error = $"Job with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteJob(int id)
    {
        try
        {
            var service = _factory.CreateJobService();
            var success = await service.DeleteJobAsync(id);

            if (!success)
                return NotFound(new { error = $"Job with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
