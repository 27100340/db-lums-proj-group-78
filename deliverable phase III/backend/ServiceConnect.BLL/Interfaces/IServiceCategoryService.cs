using ServiceConnect.Models.Entities;

namespace ServiceConnect.BLL.Interfaces;

public interface IServiceCategoryService
{
    Task<IEnumerable<ServiceCategory>> GetAllCategoriesAsync();
    Task<ServiceCategory?> GetCategoryByIdAsync(int categoryId);
}
