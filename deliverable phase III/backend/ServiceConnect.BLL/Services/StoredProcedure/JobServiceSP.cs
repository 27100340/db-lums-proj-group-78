using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ServiceConnect.BLL.Interfaces;
using ServiceConnect.Models.DTOs;
using System.Data;

namespace ServiceConnect.BLL.Services.StoredProcedure;

public class JobServiceSP : IJobService
{
    private readonly string _connectionString;

    public JobServiceSP(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ServiceConnectDB") ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<IEnumerable<JobDTO>> GetAllJobsAsync()
    {
        var jobs = new List<JobDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT TOP 100
                   j.JobID, j.CustomerID, j.CategoryID, j.Title, j.Description, j.Budget,
                   j.PostedDate, j.StartDate, j.EndDate, j.Location, j.Status, j.UrgencyLevel, j.RequiredWorkers,
                   c.FirstName + ' ' + c.LastName AS CustomerName,
                   sc.CategoryName
            FROM Jobs j
            INNER JOIN Customers c ON j.CustomerID = c.CustomerID
            INNER JOIN ServiceCategories sc ON j.CategoryID = sc.CategoryID
            ORDER BY j.PostedDate DESC", connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            jobs.Add(new JobDTO
            {
                JobID = reader.GetInt32(reader.GetOrdinal("JobID")),
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                Budget = reader.IsDBNull(reader.GetOrdinal("Budget")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Budget")),
                PostedDate = reader.GetDateTime(reader.GetOrdinal("PostedDate")),
                StartDate = reader.IsDBNull(reader.GetOrdinal("StartDate")) ? null : reader.GetDateTime(reader.GetOrdinal("StartDate")),
                EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate")) ? null : reader.GetDateTime(reader.GetOrdinal("EndDate")),
                Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? null : reader.GetString(reader.GetOrdinal("Location")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                UrgencyLevel = reader.IsDBNull(reader.GetOrdinal("UrgencyLevel")) ? null : reader.GetString(reader.GetOrdinal("UrgencyLevel")),
                RequiredWorkers = reader.GetInt32(reader.GetOrdinal("RequiredWorkers")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
            });
        }

        return jobs;
    }

    public async Task<JobDTO?> GetJobByIdAsync(int jobId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT j.JobID, j.CustomerID, j.CategoryID, j.Title, j.Description, j.Budget,
                   j.PostedDate, j.StartDate, j.EndDate, j.Location, j.Status, j.UrgencyLevel, j.RequiredWorkers,
                   c.FirstName + ' ' + c.LastName AS CustomerName,
                   sc.CategoryName
            FROM Jobs j
            INNER JOIN Customers c ON j.CustomerID = c.CustomerID
            INNER JOIN ServiceCategories sc ON j.CategoryID = sc.CategoryID
            WHERE j.JobID = @JobID", connection);

        command.Parameters.AddWithValue("@JobID", jobId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new JobDTO
            {
                JobID = reader.GetInt32(reader.GetOrdinal("JobID")),
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                Budget = reader.IsDBNull(reader.GetOrdinal("Budget")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Budget")),
                PostedDate = reader.GetDateTime(reader.GetOrdinal("PostedDate")),
                StartDate = reader.IsDBNull(reader.GetOrdinal("StartDate")) ? null : reader.GetDateTime(reader.GetOrdinal("StartDate")),
                EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate")) ? null : reader.GetDateTime(reader.GetOrdinal("EndDate")),
                Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? null : reader.GetString(reader.GetOrdinal("Location")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                UrgencyLevel = reader.IsDBNull(reader.GetOrdinal("UrgencyLevel")) ? null : reader.GetString(reader.GetOrdinal("UrgencyLevel")),
                RequiredWorkers = reader.GetInt32(reader.GetOrdinal("RequiredWorkers")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
            };
        }

        return null;
    }

    public async Task<JobDTO> CreateJobAsync(JobDTO jobDto)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            INSERT INTO Jobs (CustomerID, CategoryID, Title, Description, Budget, PostedDate, StartDate, EndDate, Location, Status, UrgencyLevel, RequiredWorkers)
            VALUES (@CustomerID, @CategoryID, @Title, @Description, @Budget, GETDATE(), @StartDate, @EndDate, @Location, 'Open', @UrgencyLevel, @RequiredWorkers);
            SELECT CAST(SCOPE_IDENTITY() AS INT);", connection);

        command.Parameters.AddWithValue("@CustomerID", jobDto.CustomerID);
        command.Parameters.AddWithValue("@CategoryID", jobDto.CategoryID);
        command.Parameters.AddWithValue("@Title", jobDto.Title);
        command.Parameters.AddWithValue("@Description", (object?)jobDto.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Budget", (object?)jobDto.Budget ?? DBNull.Value);
        command.Parameters.AddWithValue("@StartDate", (object?)jobDto.StartDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@EndDate", (object?)jobDto.EndDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@Location", (object?)jobDto.Location ?? DBNull.Value);
        command.Parameters.AddWithValue("@UrgencyLevel", (object?)jobDto.UrgencyLevel ?? DBNull.Value);
        command.Parameters.AddWithValue("@RequiredWorkers", jobDto.RequiredWorkers);

        var jobId = (int)await command.ExecuteScalarAsync()!;
        jobDto.JobID = jobId;
        jobDto.Status = "Open";
        jobDto.PostedDate = DateTime.Now;

        return jobDto;
    }

    public async Task<bool> UpdateJobAsync(JobDTO jobDto)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            UPDATE Jobs
            SET Title = @Title, Description = @Description, Budget = @Budget,
                StartDate = @StartDate, EndDate = @EndDate, Location = @Location,
                Status = @Status, UrgencyLevel = @UrgencyLevel, RequiredWorkers = @RequiredWorkers
            WHERE JobID = @JobID", connection);

        command.Parameters.AddWithValue("@JobID", jobDto.JobID);
        command.Parameters.AddWithValue("@Title", jobDto.Title);
        command.Parameters.AddWithValue("@Description", (object?)jobDto.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Budget", (object?)jobDto.Budget ?? DBNull.Value);
        command.Parameters.AddWithValue("@StartDate", (object?)jobDto.StartDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@EndDate", (object?)jobDto.EndDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@Location", (object?)jobDto.Location ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", jobDto.Status ?? "Open");
        command.Parameters.AddWithValue("@UrgencyLevel", (object?)jobDto.UrgencyLevel ?? DBNull.Value);
        command.Parameters.AddWithValue("@RequiredWorkers", jobDto.RequiredWorkers);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteJobAsync(int jobId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("DELETE FROM Jobs WHERE JobID = @JobID", connection);
        command.Parameters.AddWithValue("@JobID", jobId);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<JobDTO>> GetOpenJobsAsync()
    {
        var jobs = new List<JobDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT j.JobID, j.CustomerID, j.CategoryID, j.Title, j.Description, j.Budget,
                   j.PostedDate, j.Location, j.Status, j.UrgencyLevel, j.RequiredWorkers,
                   c.FirstName + ' ' + c.LastName AS CustomerName,
                   sc.CategoryName
            FROM Jobs j
            INNER JOIN Customers c ON j.CustomerID = c.CustomerID
            INNER JOIN ServiceCategories sc ON j.CategoryID = sc.CategoryID
            WHERE j.Status = 'Open'
            ORDER BY j.PostedDate DESC", connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            jobs.Add(new JobDTO
            {
                JobID = reader.GetInt32(reader.GetOrdinal("JobID")),
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                Budget = reader.IsDBNull(reader.GetOrdinal("Budget")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Budget")),
                PostedDate = reader.GetDateTime(reader.GetOrdinal("PostedDate")),
                Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? null : reader.GetString(reader.GetOrdinal("Location")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                UrgencyLevel = reader.IsDBNull(reader.GetOrdinal("UrgencyLevel")) ? null : reader.GetString(reader.GetOrdinal("UrgencyLevel")),
                RequiredWorkers = reader.GetInt32(reader.GetOrdinal("RequiredWorkers")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
            });
        }

        return jobs;
    }

    public async Task<IEnumerable<JobDTO>> GetJobsByCategoryAsync(int categoryId)
    {
        var jobs = new List<JobDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT j.JobID, j.CustomerID, j.CategoryID, j.Title, j.Description, j.Budget,
                   j.PostedDate, j.Location, j.Status, j.UrgencyLevel,
                   c.FirstName + ' ' + c.LastName AS CustomerName,
                   sc.CategoryName
            FROM Jobs j
            INNER JOIN Customers c ON j.CustomerID = c.CustomerID
            INNER JOIN ServiceCategories sc ON j.CategoryID = sc.CategoryID
            WHERE j.CategoryID = @CategoryID", connection);

        command.Parameters.AddWithValue("@CategoryID", categoryId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            jobs.Add(new JobDTO
            {
                JobID = reader.GetInt32(reader.GetOrdinal("JobID")),
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                Budget = reader.IsDBNull(reader.GetOrdinal("Budget")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Budget")),
                PostedDate = reader.GetDateTime(reader.GetOrdinal("PostedDate")),
                Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? null : reader.GetString(reader.GetOrdinal("Location")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                UrgencyLevel = reader.IsDBNull(reader.GetOrdinal("UrgencyLevel")) ? null : reader.GetString(reader.GetOrdinal("UrgencyLevel")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
            });
        }

        return jobs;
    }

    public async Task<IEnumerable<JobDTO>> GetJobsByCustomerAsync(int customerId)
    {
        var jobs = new List<JobDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT j.JobID, j.CustomerID, j.CategoryID, j.Title, j.Description, j.Budget,
                   j.PostedDate, j.Location, j.Status, j.UrgencyLevel,
                   sc.CategoryName
            FROM Jobs j
            INNER JOIN ServiceCategories sc ON j.CategoryID = sc.CategoryID
            WHERE j.CustomerID = @CustomerID
            ORDER BY j.PostedDate DESC", connection);

        command.Parameters.AddWithValue("@CustomerID", customerId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            jobs.Add(new JobDTO
            {
                JobID = reader.GetInt32(reader.GetOrdinal("JobID")),
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                Budget = reader.IsDBNull(reader.GetOrdinal("Budget")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Budget")),
                PostedDate = reader.GetDateTime(reader.GetOrdinal("PostedDate")),
                Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? null : reader.GetString(reader.GetOrdinal("Location")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                UrgencyLevel = reader.IsDBNull(reader.GetOrdinal("UrgencyLevel")) ? null : reader.GetString(reader.GetOrdinal("UrgencyLevel")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
            });
        }

        return jobs;
    }

    public async Task<IEnumerable<JobDTO>> GetJobsByLocationAsync(string city, int categoryId)
    {
        // tbale valued fn_GetJobsByLocation
        var jobs = new List<JobDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("SELECT * FROM dbo.fn_GetJobsByLocation(@City, @CategoryID)", connection);
        command.Parameters.AddWithValue("@City", city);
        command.Parameters.AddWithValue("@CategoryID", categoryId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            jobs.Add(new JobDTO
            {
                JobID = reader.GetInt32(reader.GetOrdinal("JobID")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Budget = reader.IsDBNull(reader.GetOrdinal("Budget")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Budget")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                UrgencyLevel = reader.IsDBNull(reader.GetOrdinal("UrgencyLevel")) ? null : reader.GetString(reader.GetOrdinal("UrgencyLevel")),
                PostedDate = reader.GetDateTime(reader.GetOrdinal("PostedDate")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
            });
        }

        return jobs;
    }

    public async Task<IEnumerable<object>> GetActiveJobsWithBidsAsync()
    {
        // view vw_ActiveJobsWithBids
        var results = new List<object>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("SELECT * FROM vw_ActiveJobsWithBids", connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new
            {
                JobID = reader.GetInt32(reader.GetOrdinal("JobID")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Budget = reader.IsDBNull(reader.GetOrdinal("Budget")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Budget")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                UrgencyLevel = reader.IsDBNull(reader.GetOrdinal("UrgencyLevel")) ? null : reader.GetString(reader.GetOrdinal("UrgencyLevel")),
                PostedDate = reader.GetDateTime(reader.GetOrdinal("PostedDate")),
                RequiredWorkers = reader.GetInt32(reader.GetOrdinal("RequiredWorkers")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                TotalBids = reader.GetInt32(reader.GetOrdinal("TotalBids")),
                AcceptedBids = reader.GetInt32(reader.GetOrdinal("AcceptedBids"))
            });
        }

        return results;
    }

    public async Task<int> CalculateJobComplexityAsync(decimal budget, string urgencyLevel, int requiredWorkers)
    {
        // scalar function fn_CalculateJobComplexity
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("SELECT dbo.fn_CalculateJobComplexity(@Budget, @UrgencyLevel, @RequiredWorkers)", connection);
        command.Parameters.AddWithValue("@Budget", budget);
        command.Parameters.AddWithValue("@UrgencyLevel", urgencyLevel);
        command.Parameters.AddWithValue("@RequiredWorkers", requiredWorkers);

        var result = await command.ExecuteScalarAsync();
        return result != null ? Convert.ToInt32(result) : 0;
    }
}
