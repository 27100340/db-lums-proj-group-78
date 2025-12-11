using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ServiceConnect.BLL.Interfaces;
using ServiceConnect.Models.DTOs;
using System.Data;

namespace ServiceConnect.BLL.Services.StoredProcedure;

public class BidServiceSP : IBidService
{
    private readonly string _connectionString;

    public BidServiceSP(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ServiceConnectDB") ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<IEnumerable<BidDTO>> GetAllBidsAsync()
    {
        var bids = new List<BidDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT TOP 100
                   b.BidID, b.JobID, b.WorkerID, b.BidAmount, b.ProposedStartTime, b.EstimatedDuration,
                   b.CoverLetter, b.BidDate, b.Status, b.IsWinningBid,
                   w.FirstName + ' ' + w.LastName AS WorkerName,
                   j.Title AS JobTitle
            FROM Bids b
            INNER JOIN Workers w ON b.WorkerID = w.WorkerID
            INNER JOIN Jobs j ON b.JobID = j.JobID
            ORDER BY b.BidDate DESC", connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bids.Add(MapBidFromReader(reader));
        }

        return bids;
    }

    public async Task<BidDTO?> GetBidByIdAsync(int bidId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT b.BidID, b.JobID, b.WorkerID, b.BidAmount, b.ProposedStartTime, b.EstimatedDuration,
                   b.CoverLetter, b.BidDate, b.Status, b.IsWinningBid,
                   w.FirstName + ' ' + w.LastName AS WorkerName,
                   j.Title AS JobTitle
            FROM Bids b
            INNER JOIN Workers w ON b.WorkerID = w.WorkerID
            INNER JOIN Jobs j ON b.JobID = j.JobID
            WHERE b.BidID = @BidID", connection);

        command.Parameters.AddWithValue("@BidID", bidId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapBidFromReader(reader);
        }

        return null;
    }

    public async Task<BidDTO> CreateBidAsync(BidDTO bidDto)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            INSERT INTO Bids (JobID, WorkerID, BidAmount, ProposedStartTime, EstimatedDuration, CoverLetter, BidDate, Status, IsWinningBid)
            VALUES (@JobID, @WorkerID, @BidAmount, @ProposedStartTime, @EstimatedDuration, @CoverLetter, GETDATE(), 'Pending', 0);
            SELECT CAST(SCOPE_IDENTITY() AS INT);", connection);

        command.Parameters.AddWithValue("@JobID", bidDto.JobID);
        command.Parameters.AddWithValue("@WorkerID", bidDto.WorkerID);
        command.Parameters.AddWithValue("@BidAmount", bidDto.BidAmount);
        command.Parameters.AddWithValue("@ProposedStartTime", (object?)bidDto.ProposedStartTime ?? DBNull.Value);
        command.Parameters.AddWithValue("@EstimatedDuration", (object?)bidDto.EstimatedDuration ?? DBNull.Value);
        command.Parameters.AddWithValue("@CoverLetter", (object?)bidDto.CoverLetter ?? DBNull.Value);

        var bidId = (int)await command.ExecuteScalarAsync()!;
        bidDto.BidID = bidId;
        bidDto.Status = "Pending";
        bidDto.BidDate = DateTime.Now;
        bidDto.IsWinningBid = false;

        return bidDto;
    }

    public async Task<bool> UpdateBidAsync(BidDTO bidDto)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            UPDATE Bids
            SET BidAmount = @BidAmount, ProposedStartTime = @ProposedStartTime,
                EstimatedDuration = @EstimatedDuration, CoverLetter = @CoverLetter, Status = @Status
            WHERE BidID = @BidID", connection);

        command.Parameters.AddWithValue("@BidID", bidDto.BidID);
        command.Parameters.AddWithValue("@BidAmount", bidDto.BidAmount);
        command.Parameters.AddWithValue("@ProposedStartTime", (object?)bidDto.ProposedStartTime ?? DBNull.Value);
        command.Parameters.AddWithValue("@EstimatedDuration", (object?)bidDto.EstimatedDuration ?? DBNull.Value);
        command.Parameters.AddWithValue("@CoverLetter", (object?)bidDto.CoverLetter ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", bidDto.Status ?? "Pending");

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteBidAsync(int bidId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("DELETE FROM Bids WHERE BidID = @BidID", connection);
        command.Parameters.AddWithValue("@BidID", bidId);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<BidDTO>> GetBidsByJobAsync(int jobId)
    {
        var bids = new List<BidDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT b.BidID, b.JobID, b.WorkerID, b.BidAmount, b.ProposedStartTime, b.EstimatedDuration,
                   b.BidDate, b.Status, b.IsWinningBid,
                   w.FirstName + ' ' + w.LastName AS WorkerName
            FROM Bids b
            INNER JOIN Workers w ON b.WorkerID = w.WorkerID
            WHERE b.JobID = @JobID
            ORDER BY b.BidDate DESC", connection);

        command.Parameters.AddWithValue("@JobID", jobId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bids.Add(new BidDTO
            {
                BidID = reader.GetInt32(reader.GetOrdinal("BidID")),
                JobID = reader.GetInt32(reader.GetOrdinal("JobID")),
                WorkerID = reader.GetInt32(reader.GetOrdinal("WorkerID")),
                BidAmount = reader.IsDBNull(reader.GetOrdinal("BidAmount")) ? 0 : reader.GetDecimal(reader.GetOrdinal("BidAmount")),
                ProposedStartTime = reader.IsDBNull(reader.GetOrdinal("ProposedStartTime")) ? null : reader.GetDateTime(reader.GetOrdinal("ProposedStartTime")),
                EstimatedDuration = reader.IsDBNull(reader.GetOrdinal("EstimatedDuration")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("EstimatedDuration")),
                BidDate = reader.GetDateTime(reader.GetOrdinal("BidDate")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                IsWinningBid = reader.GetBoolean(reader.GetOrdinal("IsWinningBid")),
                WorkerName = reader.GetString(reader.GetOrdinal("WorkerName"))
            });
        }

        return bids;
    }

    public async Task<IEnumerable<BidDTO>> GetBidsByWorkerAsync(int workerId)
    {
        var bids = new List<BidDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT b.BidID, b.JobID, b.WorkerID, b.BidAmount, b.ProposedStartTime, b.EstimatedDuration,
                   b.BidDate, b.Status, b.IsWinningBid,
                   j.Title AS JobTitle
            FROM Bids b
            INNER JOIN Jobs j ON b.JobID = j.JobID
            WHERE b.WorkerID = @WorkerID
            ORDER BY b.BidDate DESC", connection);

        command.Parameters.AddWithValue("@WorkerID", workerId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bids.Add(new BidDTO
            {
                BidID = reader.GetInt32(reader.GetOrdinal("BidID")),
                JobID = reader.GetInt32(reader.GetOrdinal("JobID")),
                WorkerID = reader.GetInt32(reader.GetOrdinal("WorkerID")),
                BidAmount = reader.IsDBNull(reader.GetOrdinal("BidAmount")) ? 0 : reader.GetDecimal(reader.GetOrdinal("BidAmount")),
                ProposedStartTime = reader.IsDBNull(reader.GetOrdinal("ProposedStartTime")) ? null : reader.GetDateTime(reader.GetOrdinal("ProposedStartTime")),
                EstimatedDuration = reader.IsDBNull(reader.GetOrdinal("EstimatedDuration")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("EstimatedDuration")),
                BidDate = reader.GetDateTime(reader.GetOrdinal("BidDate")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                IsWinningBid = reader.GetBoolean(reader.GetOrdinal("IsWinningBid")),
                JobTitle = reader.GetString(reader.GetOrdinal("JobTitle"))
            });
        }

        return bids;
    }

    public async Task<string> AcceptBidAsync(int bidId)
    {
        // Using Stored Procedure sp_AcceptBid
        // This automatically triggers trg_NotifyOnBidAccepted (AFTER UPDATE trigger)
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("sp_AcceptBid", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@BidID", bidId);

        var bookingCodeParam = new SqlParameter("@BookingCode", SqlDbType.VarChar, 20)
        {
            Direction = ParameterDirection.Output
        };
        command.Parameters.Add(bookingCodeParam);

        await command.ExecuteNonQueryAsync();

        var bookingCode = bookingCodeParam.Value?.ToString() ?? string.Empty;
        return bookingCode;
    }

    public async Task<object> GetBidStatsAsync(int jobId)
    {
        // Using Table-Valued Function fn_GetBidStats
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("SELECT * FROM dbo.fn_GetBidStats(@JobID)", connection);
        command.Parameters.AddWithValue("@JobID", jobId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new
            {
                JobID = reader.GetInt32(reader.GetOrdinal("JobID")),
                TotalBids = reader.GetInt32(reader.GetOrdinal("TotalBids")),
                AverageBidAmount = reader.IsDBNull(reader.GetOrdinal("AverageBidAmount")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("AverageBidAmount")),
                MinBidAmount = reader.IsDBNull(reader.GetOrdinal("MinBidAmount")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("MinBidAmount")),
                MaxBidAmount = reader.IsDBNull(reader.GetOrdinal("MaxBidAmount")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("MaxBidAmount")),
                AcceptedBids = reader.GetInt32(reader.GetOrdinal("AcceptedBids"))
            };
        }

        return new { JobID = jobId, TotalBids = 0 };
    }

    private BidDTO MapBidFromReader(SqlDataReader reader)
    {
        return new BidDTO
        {
            BidID = reader.GetInt32(reader.GetOrdinal("BidID")),
            JobID = reader.GetInt32(reader.GetOrdinal("JobID")),
            WorkerID = reader.GetInt32(reader.GetOrdinal("WorkerID")),
            BidAmount = reader.IsDBNull(reader.GetOrdinal("BidAmount")) ? 0 : reader.GetDecimal(reader.GetOrdinal("BidAmount")),
            ProposedStartTime = reader.IsDBNull(reader.GetOrdinal("ProposedStartTime")) ? null : reader.GetDateTime(reader.GetOrdinal("ProposedStartTime")),
            EstimatedDuration = reader.IsDBNull(reader.GetOrdinal("EstimatedDuration")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("EstimatedDuration")),
            CoverLetter = reader.IsDBNull(reader.GetOrdinal("CoverLetter")) ? null : reader.GetString(reader.GetOrdinal("CoverLetter")),
            BidDate = reader.GetDateTime(reader.GetOrdinal("BidDate")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            IsWinningBid = reader.GetBoolean(reader.GetOrdinal("IsWinningBid")),
            WorkerName = reader.GetString(reader.GetOrdinal("WorkerName")),
            JobTitle = reader.GetString(reader.GetOrdinal("JobTitle"))
        };
    }
}
