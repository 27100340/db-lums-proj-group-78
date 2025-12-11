using ServiceConnect.Models.DTOs;

namespace ServiceConnect.BLL.Interfaces;

public interface ICustomerService
{
    // CRUD Operations
    Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync();
    Task<CustomerDTO?> GetCustomerByIdAsync(int customerId);
    Task<CustomerDTO> CreateCustomerAsync(CustomerDTO customerDto);
    Task<bool> UpdateCustomerAsync(CustomerDTO customerDto);
    Task<bool> DeleteCustomerAsync(int customerId);

    // Business Logic Operations
    Task<IEnumerable<CustomerDTO>> GetCustomersByCityAsync(string city);

    // Using Views
    Task<IEnumerable<object>> GetCustomerAnalyticsAsync();
}
