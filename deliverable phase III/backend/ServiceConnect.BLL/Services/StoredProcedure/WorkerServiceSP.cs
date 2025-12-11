using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ServiceConnect.BLL.Interfaces;
using ServiceConnect.Models.DTOs;
using System.Data;

namespace ServiceConnect.BLL.Services.StoredProcedure;

public class WorkerServiceSP : IWorkerService
{
    private readonly string _connectionString;

    public WorkerServiceSP(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ServiceConnectDB") ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<IEnumerable<WorkerDTO>> GetAllWorkersAsync()
    {
        var workers = new List<WorkerDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT TOP 100
                   w.WorkerID, w.FirstName, w.LastName, w.DateOfBirth, w.Address, w.City, w.PostalCode,
                   w.HourlyRate, w.OverallRating, w.TotalJobsCompleted, w.Bio,
                   u.Email, u.PhoneNumber
            FROM Workers w
            INNER JOIN Users u ON w.WorkerID = u.UserID
            ORDER BY w.TotalJobsCompleted DESC", connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            workers.Add(MapWorkerFromReader(reader));
        }

        return workers;
    }

    public async Task<WorkerDTO?> GetWorkerByIdAsync(int workerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT w.WorkerID, w.FirstName, w.LastName, w.DateOfBirth, w.Address, w.City, w.PostalCode,
                   w.HourlyRate, w.OverallRating, w.TotalJobsCompleted, w.Bio,
                   u.Email, u.PhoneNumber
            FROM Workers w
            INNER JOIN Users u ON w.WorkerID = u.UserID
            WHERE w.WorkerID = @WorkerID", connection);

        command.Parameters.AddWithValue("@WorkerID", workerId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapWorkerFromReader(reader);
        }

        return null;
    }

    public async Task<WorkerDTO> CreateWorkerAsync(WorkerDTO workerDto)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Create User first
            var userCommand = new SqlCommand(@"
                INSERT INTO Users (Email, PasswordHash, PhoneNumber, UserType, RegistrationDate, IsVerified, AccountStatus)
                VALUES (@Email, @PasswordHash, @PhoneNumber, 'Worker', GETDATE(), 0, 'Active');
                SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction);

            userCommand.Parameters.AddWithValue("@Email", workerDto.Email);
            userCommand.Parameters.AddWithValue("@PasswordHash", BCrypt.Net.BCrypt.HashPassword(workerDto.Password ?? "defaultPassword123"));
            userCommand.Parameters.AddWithValue("@PhoneNumber", (object?)workerDto.PhoneNumber ?? DBNull.Value);

            var userId = (int)await userCommand.ExecuteScalarAsync()!;

            // Create Worker
            var workerCommand = new SqlCommand(@"
                INSERT INTO Workers (WorkerID, FirstName, LastName, DateOfBirth, Address, City, PostalCode, HourlyRate, OverallRating, TotalJobsCompleted, Bio)
                VALUES (@WorkerID, @FirstName, @LastName, @DateOfBirth, @Address, @City, @PostalCode, @HourlyRate, 0, 0, @Bio)", connection, transaction);

            workerCommand.Parameters.AddWithValue("@WorkerID", userId);
            workerCommand.Parameters.AddWithValue("@FirstName", workerDto.FirstName);
            workerCommand.Parameters.AddWithValue("@LastName", workerDto.LastName);
            workerCommand.Parameters.AddWithValue("@DateOfBirth", (object?)workerDto.DateOfBirth ?? DBNull.Value);
            workerCommand.Parameters.AddWithValue("@Address", (object?)workerDto.Address ?? DBNull.Value);
            workerCommand.Parameters.AddWithValue("@City", (object?)workerDto.City ?? DBNull.Value);
            workerCommand.Parameters.AddWithValue("@PostalCode", (object?)workerDto.PostalCode ?? DBNull.Value);
            workerCommand.Parameters.AddWithValue("@HourlyRate", (object?)workerDto.HourlyRate ?? DBNull.Value);
            workerCommand.Parameters.AddWithValue("@Bio", (object?)workerDto.Bio ?? DBNull.Value);

            await workerCommand.ExecuteNonQueryAsync();

            transaction.Commit();

            workerDto.WorkerID = userId;
            return workerDto;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateWorkerAsync(WorkerDTO workerDto)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            UPDATE Workers
            SET FirstName = @FirstName, LastName = @LastName, DateOfBirth = @DateOfBirth,
                Address = @Address, City = @City, PostalCode = @PostalCode,
                HourlyRate = @HourlyRate, Bio = @Bio
            WHERE WorkerID = @WorkerID;

            UPDATE Users
            SET PhoneNumber = @PhoneNumber
            WHERE UserID = @WorkerID", connection);

        command.Parameters.AddWithValue("@WorkerID", workerDto.WorkerID);
        command.Parameters.AddWithValue("@FirstName", workerDto.FirstName);
        command.Parameters.AddWithValue("@LastName", workerDto.LastName);
        command.Parameters.AddWithValue("@DateOfBirth", (object?)workerDto.DateOfBirth ?? DBNull.Value);
        command.Parameters.AddWithValue("@Address", (object?)workerDto.Address ?? DBNull.Value);
        command.Parameters.AddWithValue("@City", (object?)workerDto.City ?? DBNull.Value);
        command.Parameters.AddWithValue("@PostalCode", (object?)workerDto.PostalCode ?? DBNull.Value);
        command.Parameters.AddWithValue("@HourlyRate", (object?)workerDto.HourlyRate ?? DBNull.Value);
        command.Parameters.AddWithValue("@Bio", (object?)workerDto.Bio ?? DBNull.Value);
        command.Parameters.AddWithValue("@PhoneNumber", (object?)workerDto.PhoneNumber ?? DBNull.Value);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteWorkerAsync(int workerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("DELETE FROM Workers WHERE WorkerID = @WorkerID", connection);
        command.Parameters.AddWithValue("@WorkerID", workerId);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<WorkerDTO>> GetWorkersBySkillAsync(int categoryId)
    {
        var workers = new List<WorkerDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT DISTINCT w.WorkerID, w.FirstName, w.LastName, w.City, w.HourlyRate, w.OverallRating, w.TotalJobsCompleted,
                   u.Email
            FROM Workers w
            INNER JOIN Users u ON w.WorkerID = u.UserID
            INNER JOIN WorkerSkills ws ON w.WorkerID = ws.WorkerID
            WHERE ws.CategoryID = @CategoryID", connection);

        command.Parameters.AddWithValue("@CategoryID", categoryId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            workers.Add(new WorkerDTO
            {
                WorkerID = reader.GetInt32(reader.GetOrdinal("WorkerID")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City")),
                HourlyRate = reader.IsDBNull(reader.GetOrdinal("HourlyRate")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("HourlyRate")),
                OverallRating = reader.IsDBNull(reader.GetOrdinal("OverallRating")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("OverallRating")),
                TotalJobsCompleted = reader.GetInt32(reader.GetOrdinal("TotalJobsCompleted"))
            });
        }

        return workers;
    }

    public async Task<IEnumerable<WorkerDTO>> GetWorkersByCityAsync(string city)
    {
        var workers = new List<WorkerDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT w.WorkerID, w.FirstName, w.LastName, w.City, w.HourlyRate, w.OverallRating, w.TotalJobsCompleted,
                   u.Email
            FROM Workers w
            INNER JOIN Users u ON w.WorkerID = u.UserID
            WHERE w.City = @City", connection);

        command.Parameters.AddWithValue("@City", city);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            workers.Add(new WorkerDTO
            {
                WorkerID = reader.GetInt32(reader.GetOrdinal("WorkerID")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                City = reader.GetString(reader.GetOrdinal("City")),
                HourlyRate = reader.IsDBNull(reader.GetOrdinal("HourlyRate")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("HourlyRate")),
                OverallRating = reader.IsDBNull(reader.GetOrdinal("OverallRating")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("OverallRating")),
                TotalJobsCompleted = reader.GetInt32(reader.GetOrdinal("TotalJobsCompleted"))
            });
        }

        return workers;
    }

    public async Task<IEnumerable<object>> GetAvailableWorkersForJobAsync(int jobId, int categoryId)
    {
        // Using Stored Procedure sp_GetAvailableWorkers
        var workers = new List<object>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("sp_GetAvailableWorkers", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@JobID", jobId);
        command.Parameters.AddWithValue("@CategoryID", categoryId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            workers.Add(new
            {
                WorkerID = reader.GetInt32(reader.GetOrdinal("WorkerID")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                HourlyRate = reader.IsDBNull(reader.GetOrdinal("HourlyRate")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("HourlyRate")),
                OverallRating = reader.IsDBNull(reader.GetOrdinal("OverallRating")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("OverallRating")),
                TotalJobsCompleted = reader.GetInt32(reader.GetOrdinal("TotalJobsCompleted")),
                SkillLevel = reader.IsDBNull(reader.GetOrdinal("SkillLevel")) ? null : reader.GetString(reader.GetOrdinal("SkillLevel")),
                YearsExperience = reader.IsDBNull(reader.GetOrdinal("YearsExperience")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("YearsExperience"))
            });
        }

        return workers;
    }

    public async Task<object> GetWorkerPerformanceAsync(int workerId)
    {
        // Using Stored Procedure sp_GetWorkerPerformance
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("sp_GetWorkerPerformance", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@WorkerID", workerId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new
            {
                WorkerID = reader.GetInt32(reader.GetOrdinal("WorkerID")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                HourlyRate = reader.IsDBNull(reader.GetOrdinal("HourlyRate")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("HourlyRate")),
                OverallRating = reader.IsDBNull(reader.GetOrdinal("OverallRating")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("OverallRating")),
                TotalJobsCompleted = reader.GetInt32(reader.GetOrdinal("TotalJobsCompleted")),
                TotalBidsPlaced = reader.GetInt32(reader.GetOrdinal("TotalBidsPlaced")),
                WinningBids = reader.GetInt32(reader.GetOrdinal("WinningBids")),
                AverageRating = reader.IsDBNull(reader.GetOrdinal("AverageRating")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("AverageRating")),
                TotalReviews = reader.GetInt32(reader.GetOrdinal("TotalReviews"))
            };
        }

        return new { };
    }

    public async Task<IEnumerable<object>> GetTopPerformersByCategoryAsync(int categoryId)
    {
        // Using Stored Procedure sp_TopPerformersByCategory (uses CTE)
        var performers = new List<object>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("sp_TopPerformersByCategory", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@CategoryID", categoryId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            performers.Add(new
            {
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                HourlyRate = reader.IsDBNull(reader.GetOrdinal("HourlyRate")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("HourlyRate")),
                OverallRating = reader.IsDBNull(reader.GetOrdinal("OverallRating")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("OverallRating")),
                TotalJobsCompleted = reader.GetInt32(reader.GetOrdinal("TotalJobsCompleted")),
                TotalBids = reader.GetInt32(reader.GetOrdinal("TotalBids")),
                WinningBids = reader.GetInt32(reader.GetOrdinal("WinningBids")),
                WinRatePercentage = reader.IsDBNull(reader.GetOrdinal("WinRatePercentage")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("WinRatePercentage")),
                PerformanceRank = reader.GetInt64(reader.GetOrdinal("PerformanceRank"))
            });
        }

        return performers;
    }

    public async Task<decimal> GetWorkerReliabilityScoreAsync(int workerId)
    {
        // Using Scalar Function fn_GetWorkerReliabilityScore
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("SELECT dbo.fn_GetWorkerReliabilityScore(@WorkerID)", connection);
        command.Parameters.AddWithValue("@WorkerID", workerId);

        var result = await command.ExecuteScalarAsync();
        return result != null ? Convert.ToDecimal(result) : 0;
    }

    public async Task<IEnumerable<object>> GetTopRatedWorkersAsync()
    {
        // Using View vw_TopRatedWorkers
        var workers = new List<object>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("SELECT * FROM vw_TopRatedWorkers ORDER BY OverallRating DESC", connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            workers.Add(new
            {
                WorkerID = reader.GetInt32(reader.GetOrdinal("WorkerID")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                HourlyRate = reader.IsDBNull(reader.GetOrdinal("HourlyRate")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("HourlyRate")),
                OverallRating = reader.IsDBNull(reader.GetOrdinal("OverallRating")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("OverallRating")),
                TotalJobsCompleted = reader.GetInt32(reader.GetOrdinal("TotalJobsCompleted")),
                City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City")),
                CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? null : reader.GetString(reader.GetOrdinal("CategoryName")),
                SkillLevel = reader.IsDBNull(reader.GetOrdinal("SkillLevel")) ? null : reader.GetString(reader.GetOrdinal("SkillLevel")),
                ReviewCount = reader.GetInt32(reader.GetOrdinal("ReviewCount")),
                AverageRating = reader.IsDBNull(reader.GetOrdinal("AverageRating")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("AverageRating"))
            });
        }

        return workers;
    }

    private WorkerDTO MapWorkerFromReader(SqlDataReader reader)
    {
        return new WorkerDTO
        {
            WorkerID = reader.GetInt32(reader.GetOrdinal("WorkerID")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
            DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? null : reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
            Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
            City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City")),
            PostalCode = reader.IsDBNull(reader.GetOrdinal("PostalCode")) ? null : reader.GetString(reader.GetOrdinal("PostalCode")),
            HourlyRate = reader.IsDBNull(reader.GetOrdinal("HourlyRate")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("HourlyRate")),
            OverallRating = reader.IsDBNull(reader.GetOrdinal("OverallRating")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("OverallRating")),
            TotalJobsCompleted = reader.GetInt32(reader.GetOrdinal("TotalJobsCompleted")),
            Bio = reader.IsDBNull(reader.GetOrdinal("Bio")) ? null : reader.GetString(reader.GetOrdinal("Bio"))
        };
    }
}
