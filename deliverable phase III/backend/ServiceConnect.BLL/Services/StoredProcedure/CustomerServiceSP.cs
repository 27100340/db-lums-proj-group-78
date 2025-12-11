using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ServiceConnect.BLL.Interfaces;
using ServiceConnect.Models.DTOs;

namespace ServiceConnect.BLL.Services.StoredProcedure;

public class CustomerServiceSP : ICustomerService
{
    private readonly string _connectionString;

    public CustomerServiceSP(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ServiceConnectDB") ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync()
    {
        var customers = new List<CustomerDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT TOP 100
                   c.CustomerID, c.FirstName, c.LastName, c.Address, c.City, c.PostalCode, c.CustomerRating, c.TotalJobsPosted,
                   u.Email, u.PhoneNumber
            FROM Customers c
            INNER JOIN Users u ON c.CustomerID = u.UserID
            ORDER BY c.TotalJobsPosted DESC", connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            customers.Add(MapCustomerFromReader(reader));
        }

        return customers;
    }

    public async Task<CustomerDTO?> GetCustomerByIdAsync(int customerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT c.CustomerID, c.FirstName, c.LastName, c.Address, c.City, c.PostalCode, c.CustomerRating, c.TotalJobsPosted,
                   u.Email, u.PhoneNumber
            FROM Customers c
            INNER JOIN Users u ON c.CustomerID = u.UserID
            WHERE c.CustomerID = @CustomerID", connection);

        command.Parameters.AddWithValue("@CustomerID", customerId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapCustomerFromReader(reader);
        }

        return null;
    }

    public async Task<CustomerDTO> CreateCustomerAsync(CustomerDTO customerDto)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Create User first
            var userCommand = new SqlCommand(@"
                INSERT INTO Users (Email, PasswordHash, PhoneNumber, UserType, RegistrationDate, IsVerified, AccountStatus)
                VALUES (@Email, @PasswordHash, @PhoneNumber, 'Customer', GETDATE(), 0, 'Active');
                SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction);

            userCommand.Parameters.AddWithValue("@Email", customerDto.Email);
            userCommand.Parameters.AddWithValue("@PasswordHash", BCrypt.Net.BCrypt.HashPassword(customerDto.Password ?? "defaultPassword123"));
            userCommand.Parameters.AddWithValue("@PhoneNumber", (object?)customerDto.PhoneNumber ?? DBNull.Value);

            var userId = (int)await userCommand.ExecuteScalarAsync()!;

            // Create Customer
            var customerCommand = new SqlCommand(@"
                INSERT INTO Customers (CustomerID, FirstName, LastName, Address, City, PostalCode, CustomerRating, TotalJobsPosted)
                VALUES (@CustomerID, @FirstName, @LastName, @Address, @City, @PostalCode, 0, 0)", connection, transaction);

            customerCommand.Parameters.AddWithValue("@CustomerID", userId);
            customerCommand.Parameters.AddWithValue("@FirstName", customerDto.FirstName);
            customerCommand.Parameters.AddWithValue("@LastName", customerDto.LastName);
            customerCommand.Parameters.AddWithValue("@Address", (object?)customerDto.Address ?? DBNull.Value);
            customerCommand.Parameters.AddWithValue("@City", (object?)customerDto.City ?? DBNull.Value);
            customerCommand.Parameters.AddWithValue("@PostalCode", (object?)customerDto.PostalCode ?? DBNull.Value);

            await customerCommand.ExecuteNonQueryAsync();

            transaction.Commit();

            customerDto.CustomerID = userId;
            return customerDto;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateCustomerAsync(CustomerDTO customerDto)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            UPDATE Customers
            SET FirstName = @FirstName, LastName = @LastName, Address = @Address,
                City = @City, PostalCode = @PostalCode
            WHERE CustomerID = @CustomerID;

            UPDATE Users
            SET PhoneNumber = @PhoneNumber
            WHERE UserID = @CustomerID", connection);

        command.Parameters.AddWithValue("@CustomerID", customerDto.CustomerID);
        command.Parameters.AddWithValue("@FirstName", customerDto.FirstName);
        command.Parameters.AddWithValue("@LastName", customerDto.LastName);
        command.Parameters.AddWithValue("@Address", (object?)customerDto.Address ?? DBNull.Value);
        command.Parameters.AddWithValue("@City", (object?)customerDto.City ?? DBNull.Value);
        command.Parameters.AddWithValue("@PostalCode", (object?)customerDto.PostalCode ?? DBNull.Value);
        command.Parameters.AddWithValue("@PhoneNumber", (object?)customerDto.PhoneNumber ?? DBNull.Value);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteCustomerAsync(int customerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("DELETE FROM Customers WHERE CustomerID = @CustomerID", connection);
        command.Parameters.AddWithValue("@CustomerID", customerId);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<CustomerDTO>> GetCustomersByCityAsync(string city)
    {
        var customers = new List<CustomerDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT c.CustomerID, c.FirstName, c.LastName, c.City, c.CustomerRating, c.TotalJobsPosted,
                   u.Email
            FROM Customers c
            INNER JOIN Users u ON c.CustomerID = u.UserID
            WHERE c.City = @City", connection);

        command.Parameters.AddWithValue("@City", city);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            customers.Add(new CustomerDTO
            {
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                City = reader.GetString(reader.GetOrdinal("City")),
                CustomerRating = reader.IsDBNull(reader.GetOrdinal("CustomerRating")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("CustomerRating")),
                TotalJobsPosted = reader.GetInt32(reader.GetOrdinal("TotalJobsPosted"))
            });
        }

        return customers;
    }

    public async Task<IEnumerable<object>> GetCustomerAnalyticsAsync()
    {
        // Using View vw_CustomerAnalytics
        var analytics = new List<object>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("SELECT * FROM vw_CustomerAnalytics", connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            analytics.Add(new
            {
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City")),
                CustomerRating = reader.IsDBNull(reader.GetOrdinal("CustomerRating")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("CustomerRating")),
                TotalJobsPosted = reader.GetInt32(reader.GetOrdinal("TotalJobsPosted")),
                ActiveJobs = reader.GetInt32(reader.GetOrdinal("ActiveJobs")),
                CompletedJobs = reader.GetInt32(reader.GetOrdinal("CompletedJobs")),
                TotalSpend = reader.IsDBNull(reader.GetOrdinal("TotalSpend")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("TotalSpend"))
            });
        }

        return analytics;
    }

    private CustomerDTO MapCustomerFromReader(SqlDataReader reader)
    {
        return new CustomerDTO
        {
            CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
            Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
            City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City")),
            PostalCode = reader.IsDBNull(reader.GetOrdinal("PostalCode")) ? null : reader.GetString(reader.GetOrdinal("PostalCode")),
            CustomerRating = reader.IsDBNull(reader.GetOrdinal("CustomerRating")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("CustomerRating")),
            TotalJobsPosted = reader.GetInt32(reader.GetOrdinal("TotalJobsPosted"))
        };
    }
}
