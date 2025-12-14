using Microsoft.EntityFrameworkCore;
using ServiceConnect.BLL.Data;
using ServiceConnect.BLL.Interfaces;
using ServiceConnect.Models.Entities;

namespace ServiceConnect.BLL.Services.LinqEF;

public class ServiceCategoryServiceEF : IServiceCategoryService
{
    private readonly ServiceConnectDbContext _context;

    public ServiceCategoryServiceEF(ServiceConnectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ServiceCategory>> GetAllCategoriesAsync()
    {
        return await _context.ServiceCategories
            .Where(sc => sc.IsActive)
            .OrderBy(sc => sc.CategoryName)
            .ToListAsync();
    }

    public async Task<ServiceCategory?> GetCategoryByIdAsync(int categoryId)
    {
        return await _context.ServiceCategories
            .FirstOrDefaultAsync(sc => sc.CategoryID == categoryId);
    }
}
