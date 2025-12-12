using Microsoft.EntityFrameworkCore;
using ServiceConnect.BLL.Data;
using ServiceConnect.BLL.Interfaces;
using ServiceConnect.Models.DTOs;
using ServiceConnect.Models.Entities;

namespace ServiceConnect.BLL.Services.LinqEF;

public class CustomerServiceEF : ICustomerService
{
    private readonly ServiceConnectDbContext _context;

    public CustomerServiceEF(ServiceConnectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync()
    {
        return await _context.Customers
            .Include(c => c.User)
            .Select(c => new CustomerDTO
            {
                CustomerID = c.CustomerID,
                Email = c.User.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                PhoneNumber = c.User.PhoneNumber,
                Address = c.Address,
                City = c.City,
                PostalCode = c.PostalCode,
                CustomerRating = c.CustomerRating,
                TotalJobsPosted = c.TotalJobsPosted
            })
            .OrderByDescending(c => c.TotalJobsPosted)
            .Take(100)
            .ToListAsync();
    }

    public async Task<CustomerDTO?> GetCustomerByIdAsync(int customerId)
    {
        var customer = await _context.Customers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CustomerID == customerId);

        if (customer == null) return null;

        return new CustomerDTO
        {
            CustomerID = customer.CustomerID,
            Email = customer.User.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            PhoneNumber = customer.User.PhoneNumber,
            Address = customer.Address,
            City = customer.City,
            PostalCode = customer.PostalCode,
            CustomerRating = customer.CustomerRating,
            TotalJobsPosted = customer.TotalJobsPosted
        };
    }

    public async Task<CustomerDTO> CreateCustomerAsync(CustomerDTO customerDto)
    {
        // make user
        var user = new User
        {
            Email = customerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(customerDto.Password ?? "defaultPassword123"),
            PhoneNumber = customerDto.PhoneNumber,
            UserType = "Customer",
            RegistrationDate = DateTime.Now,
            IsVerified = false,
            AccountStatus = "Active"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // make customer
        var customer = new Customer
        {
            CustomerID = user.UserID,
            FirstName = customerDto.FirstName,
            LastName = customerDto.LastName,
            Address = customerDto.Address,
            City = customerDto.City,
            PostalCode = customerDto.PostalCode,
            CustomerRating = 0,
            TotalJobsPosted = 0
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        customerDto.CustomerID = customer.CustomerID;
        return customerDto;
    }

    public async Task<bool> UpdateCustomerAsync(CustomerDTO customerDto)
    {
        var customer = await _context.Customers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CustomerID == customerDto.CustomerID);

        if (customer == null) return false;

        customer.FirstName = customerDto.FirstName;
        customer.LastName = customerDto.LastName;
        customer.Address = customerDto.Address;
        customer.City = customerDto.City;
        customer.PostalCode = customerDto.PostalCode;
        customer.User.PhoneNumber = customerDto.PhoneNumber;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCustomerAsync(int customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null) return false;

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CustomerDTO>> GetCustomersByCityAsync(string city)
    {
        return await _context.Customers
            .Include(c => c.User)
            .Where(c => c.City == city)
            .Select(c => new CustomerDTO
            {
                CustomerID = c.CustomerID,
                Email = c.User.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                City = c.City,
                CustomerRating = c.CustomerRating,
                TotalJobsPosted = c.TotalJobsPosted
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<object>> GetCustomerAnalyticsAsync()
    {
        // Use the view query vw_CustomerAnalytics
        var analytics = await (from c in _context.Customers
                               let activeJobs = _context.Jobs.Where(j => j.CustomerID == c.CustomerID && (j.Status == "Open" || j.Status == "Assigned"))
                               let completedBookings = _context.Bookings.Where(b => b.Job.CustomerID == c.CustomerID && b.Status == "Completed")
                               select new
                               {
                                   c.CustomerID,
                                   CustomerName = c.FirstName + " " + c.LastName,
                                   c.City,
                                   c.CustomerRating,
                                   c.TotalJobsPosted,
                                   ActiveJobs = activeJobs.Count(),
                                   CompletedJobs = completedBookings.Count(),
                                   TotalSpend = _context.Jobs.Where(j => j.CustomerID == c.CustomerID).Sum(j => (decimal?)j.Budget) ?? 0
                               }).ToListAsync();

        return analytics;
    }
}
