using ServiceConnect.Models.DTOs;

namespace ServiceConnect.BLL.Interfaces;

public interface ICustomerService
{
    // create,read,update,delete
    Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync();
    Task<CustomerDTO?> GetCustomerByIdAsync(int customerId);
    Task<CustomerDTO> CreateCustomerAsync(CustomerDTO customerDto);
    Task<bool> UpdateCustomerAsync(CustomerDTO customerDto);
    Task<bool> DeleteCustomerAsync(int customerId);

    // logic Operations
    Task<IEnumerable<CustomerDTO>> GetCustomersByCityAsync(string city);

    // views
    Task<IEnumerable<object>> GetCustomerAnalyticsAsync();
}
