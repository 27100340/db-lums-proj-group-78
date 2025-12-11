using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ServiceConnect.BLL.Interfaces;
using ServiceConnect.Models.Entities;

namespace ServiceConnect.BLL.Services.StoredProcedure;

public class ServiceCategoryServiceSP : IServiceCategoryService
{
    private readonly string _connectionString;

    public ServiceCategoryServiceSP(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ServiceConnectDB") ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<IEnumerable<ServiceCategory>> GetAllCategoriesAsync()
    {
        var categories = new List<ServiceCategory>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT CategoryID, CategoryName, CategoryDescription, IconURL, BaseRate, IsActive
            FROM ServiceCategories
            WHERE IsActive = 1
            ORDER BY CategoryName", connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            categories.Add(new ServiceCategory
            {
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                CategoryDescription = reader.IsDBNull(reader.GetOrdinal("CategoryDescription")) ? null : reader.GetString(reader.GetOrdinal("CategoryDescription")),
                IconURL = reader.IsDBNull(reader.GetOrdinal("IconURL")) ? null : reader.GetString(reader.GetOrdinal("IconURL")),
                BaseRate = reader.IsDBNull(reader.GetOrdinal("BaseRate")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("BaseRate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            });
        }

        return categories;
    }

    public async Task<ServiceCategory?> GetCategoryByIdAsync(int categoryId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT CategoryID, CategoryName, CategoryDescription, IconURL, BaseRate, IsActive
            FROM ServiceCategories
            WHERE CategoryID = @CategoryID", connection);

        command.Parameters.AddWithValue("@CategoryID", categoryId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ServiceCategory
            {
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                CategoryDescription = reader.IsDBNull(reader.GetOrdinal("CategoryDescription")) ? null : reader.GetString(reader.GetOrdinal("CategoryDescription")),
                IconURL = reader.IsDBNull(reader.GetOrdinal("IconURL")) ? null : reader.GetString(reader.GetOrdinal("IconURL")),
                BaseRate = reader.IsDBNull(reader.GetOrdinal("BaseRate")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("BaseRate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }

        return null;
    }
}
