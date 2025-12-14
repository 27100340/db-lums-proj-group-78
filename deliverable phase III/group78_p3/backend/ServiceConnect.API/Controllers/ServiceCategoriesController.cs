using Microsoft.AspNetCore.Mvc;
using ServiceConnect.BLL.Factories;
using ServiceConnect.Models.Entities;

namespace ServiceConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceCategoriesController : ControllerBase
{
    private readonly ServiceFactory _factory;

    public ServiceCategoriesController(ServiceFactory factory)
    {
        _factory = factory;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceCategory>>> GetAllCategories()
    {
        try
        {
            var service = _factory.CreateServiceCategoryService();
            var categories = await service.GetAllCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceCategory>> GetCategory(int id)
    {
        try
        {
            var service = _factory.CreateServiceCategoryService();
            var category = await service.GetCategoryByIdAsync(id);

            if (category == null)
                return NotFound(new { error = $"Category with ID {id} not found" });

            return Ok(category);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
