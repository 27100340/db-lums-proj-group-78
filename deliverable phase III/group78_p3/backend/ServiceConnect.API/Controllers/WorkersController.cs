using Microsoft.AspNetCore.Mvc;
using ServiceConnect.BLL.Factories;
using ServiceConnect.Models.DTOs;

namespace ServiceConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkersController : ControllerBase
{
    private readonly ServiceFactory _factory;

    public WorkersController(ServiceFactory factory)
    {
        _factory = factory;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkerDTO>>> GetAllWorkers()
    {
        try
        {
            var service = _factory.CreateWorkerService();
            var workers = await service.GetAllWorkersAsync();
            return Ok(workers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkerDTO>> GetWorker(int id)
    {
        try
        {
            var service = _factory.CreateWorkerService();
            var worker = await service.GetWorkerByIdAsync(id);

            if (worker == null)
                return NotFound(new { error = $"Worker with ID {id} not found" });

            return Ok(worker);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("skill/{categoryId}")]
    public async Task<ActionResult<IEnumerable<WorkerDTO>>> GetWorkersBySkill(int categoryId)
    {
        try
        {
            var service = _factory.CreateWorkerService();
            var workers = await service.GetWorkersBySkillAsync(categoryId);
            return Ok(workers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("city/{city}")]
    public async Task<ActionResult<IEnumerable<WorkerDTO>>> GetWorkersByCity(string city)
    {
        try
        {
            var service = _factory.CreateWorkerService();
            var workers = await service.GetWorkersByCityAsync(city);
            return Ok(workers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("available/{jobId}/category/{categoryId}")]
    public async Task<ActionResult> GetAvailableWorkers(int jobId, int categoryId)
    {
        try
        {
            var service = _factory.CreateWorkerService();
            var workers = await service.GetAvailableWorkersForJobAsync(jobId, categoryId);
            return Ok(workers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}/performance")]
    public async Task<ActionResult> GetWorkerPerformance(int id)
    {
        try
        {
            var service = _factory.CreateWorkerService();
            var performance = await service.GetWorkerPerformanceAsync(id);
            return Ok(performance);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("top-performers/category/{categoryId}")]
    public async Task<ActionResult> GetTopPerformers(int categoryId)
    {
        try
        {
            var service = _factory.CreateWorkerService();
            var performers = await service.GetTopPerformersByCategoryAsync(categoryId);
            return Ok(performers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}/reliability")]
    public async Task<ActionResult<decimal>> GetReliabilityScore(int id)
    {
        try
        {
            var service = _factory.CreateWorkerService();
            var score = await service.GetWorkerReliabilityScoreAsync(id);
            return Ok(new { workerId = id, reliabilityScore = score });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("top-rated")]
    public async Task<ActionResult> GetTopRatedWorkers()
    {
        try
        {
            var service = _factory.CreateWorkerService();
            var workers = await service.GetTopRatedWorkersAsync();
            return Ok(workers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<WorkerDTO>> CreateWorker([FromBody] WorkerDTO workerDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = _factory.CreateWorkerService();
            var createdWorker = await service.CreateWorkerAsync(workerDto);
            return CreatedAtAction(nameof(GetWorker), new { id = createdWorker.WorkerID }, createdWorker);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateWorker(int id, [FromBody] WorkerDTO workerDto)
    {
        try
        {
            if (workerDto.WorkerID != id)
                return BadRequest(new { error = "Worker ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = _factory.CreateWorkerService();
            var success = await service.UpdateWorkerAsync(workerDto);

            if (!success)
                return NotFound(new { error = $"Worker with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWorker(int id)
    {
        try
        {
            var service = _factory.CreateWorkerService();
            var success = await service.DeleteWorkerAsync(id);

            if (!success)
                return NotFound(new { error = $"Worker with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
