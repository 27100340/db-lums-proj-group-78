using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ServiceConnect.BLL.Interfaces;
using ServiceConnect.Models.DTOs;
using System.Data;

namespace ServiceConnect.BLL.Services.StoredProcedure;

public class BookingServiceSP : IBookingService
{
    private readonly string _connectionString;

    public BookingServiceSP(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ServiceConnectDB") ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<IEnumerable<BookingDTO>> GetAllBookingsAsync()
    {
        var bookings = new List<BookingDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT TOP 100
                   b.BookingID, b.JobID, b.WorkerID, b.BidID, b.ScheduledStart, b.ScheduledEnd,
                   b.ActualStart, b.ActualEnd, b.Status, b.CancellationReason, b.BookingCode, b.CompletionNotes,
                   j.Title AS JobTitle,
                   w.FirstName + ' ' + w.LastName AS WorkerName,
                   c.FirstName + ' ' + c.LastName AS CustomerName
            FROM Bookings b
            INNER JOIN Jobs j ON b.JobID = j.JobID
            INNER JOIN Workers w ON b.WorkerID = w.WorkerID
            INNER JOIN Customers c ON j.CustomerID = c.CustomerID
            ORDER BY b.ScheduledStart DESC", connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bookings.Add(MapBookingFromReader(reader));
        }

        return bookings;
    }

    public async Task<BookingDTO?> GetBookingByIdAsync(int bookingId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT b.BookingID, b.JobID, b.WorkerID, b.BidID, b.ScheduledStart, b.ScheduledEnd,
                   b.ActualStart, b.ActualEnd, b.Status, b.CancellationReason, b.BookingCode, b.CompletionNotes,
                   j.Title AS JobTitle,
                   w.FirstName + ' ' + w.LastName AS WorkerName,
                   c.FirstName + ' ' + c.LastName AS CustomerName
            FROM Bookings b
            INNER JOIN Jobs j ON b.JobID = j.JobID
            INNER JOIN Workers w ON b.WorkerID = w.WorkerID
            INNER JOIN Customers c ON j.CustomerID = c.CustomerID
            WHERE b.BookingID = @BookingID", connection);

        command.Parameters.AddWithValue("@BookingID", bookingId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapBookingFromReader(reader);
        }

        return null;
    }

    public async Task<BookingDTO> CreateBookingAsync(BookingDTO bookingDto)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var bookingCode = "BK" + new Random().Next(100000, 999999).ToString();

        var command = new SqlCommand(@"
            INSERT INTO Bookings (JobID, WorkerID, BidID, ScheduledStart, ScheduledEnd, Status, BookingCode)
            VALUES (@JobID, @WorkerID, @BidID, @ScheduledStart, @ScheduledEnd, 'Scheduled', @BookingCode);
            SELECT CAST(SCOPE_IDENTITY() AS INT);", connection);

        command.Parameters.AddWithValue("@JobID", bookingDto.JobID);
        command.Parameters.AddWithValue("@WorkerID", bookingDto.WorkerID);
        command.Parameters.AddWithValue("@BidID", (object?)bookingDto.BidID ?? DBNull.Value);
        command.Parameters.AddWithValue("@ScheduledStart", (object?)bookingDto.ScheduledStart ?? DBNull.Value);
        command.Parameters.AddWithValue("@ScheduledEnd", (object?)bookingDto.ScheduledEnd ?? DBNull.Value);
        command.Parameters.AddWithValue("@BookingCode", bookingCode);

        var bookingId = (int)await command.ExecuteScalarAsync()!;
        bookingDto.BookingID = bookingId;
        bookingDto.BookingCode = bookingCode;
        bookingDto.Status = "Scheduled";

        return bookingDto;
    }

    public async Task<bool> UpdateBookingAsync(BookingDTO bookingDto)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            UPDATE Bookings
            SET ScheduledStart = @ScheduledStart, ScheduledEnd = @ScheduledEnd,
                ActualStart = @ActualStart, ActualEnd = @ActualEnd, Status = @Status,
                CancellationReason = @CancellationReason, CompletionNotes = @CompletionNotes
            WHERE BookingID = @BookingID", connection);

        command.Parameters.AddWithValue("@BookingID", bookingDto.BookingID);
        command.Parameters.AddWithValue("@ScheduledStart", (object?)bookingDto.ScheduledStart ?? DBNull.Value);
        command.Parameters.AddWithValue("@ScheduledEnd", (object?)bookingDto.ScheduledEnd ?? DBNull.Value);
        command.Parameters.AddWithValue("@ActualStart", (object?)bookingDto.ActualStart ?? DBNull.Value);
        command.Parameters.AddWithValue("@ActualEnd", (object?)bookingDto.ActualEnd ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", bookingDto.Status ?? "Scheduled");
        command.Parameters.AddWithValue("@CancellationReason", (object?)bookingDto.CancellationReason ?? DBNull.Value);
        command.Parameters.AddWithValue("@CompletionNotes", (object?)bookingDto.CompletionNotes ?? DBNull.Value);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteBookingAsync(int bookingId)
    {
        // This will trigger trg_PreventDeleteCompletedBooking (INSTEAD OF DELETE trigger)
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        try
        {
            var command = new SqlCommand("DELETE FROM Bookings WHERE BookingID = @BookingID", connection);
            command.Parameters.AddWithValue("@BookingID", bookingId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (SqlException ex) when (ex.Message.Contains("Cannot delete completed bookings"))
        {
            throw new InvalidOperationException("Cannot delete completed bookings for audit purposes");
        }
    }

    public async Task<IEnumerable<BookingDTO>> GetBookingsByWorkerAsync(int workerId)
    {
        var bookings = new List<BookingDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT b.BookingID, b.JobID, b.WorkerID, b.ScheduledStart, b.ScheduledEnd, b.Status, b.BookingCode,
                   j.Title AS JobTitle,
                   c.FirstName + ' ' + c.LastName AS CustomerName
            FROM Bookings b
            INNER JOIN Jobs j ON b.JobID = j.JobID
            INNER JOIN Customers c ON j.CustomerID = c.CustomerID
            WHERE b.WorkerID = @WorkerID
            ORDER BY b.ScheduledStart DESC", connection);

        command.Parameters.AddWithValue("@WorkerID", workerId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bookings.Add(new BookingDTO
            {
                BookingID = reader.GetInt32(reader.GetOrdinal("BookingID")),
                JobID = reader.GetInt32(reader.GetOrdinal("JobID")),
                WorkerID = reader.GetInt32(reader.GetOrdinal("WorkerID")),
                ScheduledStart = reader.IsDBNull(reader.GetOrdinal("ScheduledStart")) ? null : reader.GetDateTime(reader.GetOrdinal("ScheduledStart")),
                ScheduledEnd = reader.IsDBNull(reader.GetOrdinal("ScheduledEnd")) ? null : reader.GetDateTime(reader.GetOrdinal("ScheduledEnd")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                BookingCode = reader.IsDBNull(reader.GetOrdinal("BookingCode")) ? null : reader.GetString(reader.GetOrdinal("BookingCode")),
                JobTitle = reader.GetString(reader.GetOrdinal("JobTitle")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName"))
            });
        }

        return bookings;
    }

    public async Task<IEnumerable<BookingDTO>> GetBookingsByCustomerAsync(int customerId)
    {
        var bookings = new List<BookingDTO>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT b.BookingID, b.JobID, b.WorkerID, b.ScheduledStart, b.ScheduledEnd, b.Status, b.BookingCode,
                   j.Title AS JobTitle,
                   w.FirstName + ' ' + w.LastName AS WorkerName
            FROM Bookings b
            INNER JOIN Jobs j ON b.JobID = j.JobID
            INNER JOIN Workers w ON b.WorkerID = w.WorkerID
            WHERE j.CustomerID = @CustomerID
            ORDER BY b.ScheduledStart DESC", connection);

        command.Parameters.AddWithValue("@CustomerID", customerId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bookings.Add(new BookingDTO
            {
                BookingID = reader.GetInt32(reader.GetOrdinal("BookingID")),
                JobID = reader.GetInt32(reader.GetOrdinal("JobID")),
                WorkerID = reader.GetInt32(reader.GetOrdinal("WorkerID")),
                ScheduledStart = reader.IsDBNull(reader.GetOrdinal("ScheduledStart")) ? null : reader.GetDateTime(reader.GetOrdinal("ScheduledStart")),
                ScheduledEnd = reader.IsDBNull(reader.GetOrdinal("ScheduledEnd")) ? null : reader.GetDateTime(reader.GetOrdinal("ScheduledEnd")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                BookingCode = reader.IsDBNull(reader.GetOrdinal("BookingCode")) ? null : reader.GetString(reader.GetOrdinal("BookingCode")),
                JobTitle = reader.GetString(reader.GetOrdinal("JobTitle")),
                WorkerName = reader.GetString(reader.GetOrdinal("WorkerName"))
            });
        }

        return bookings;
    }

    public async Task<bool> CompleteBookingAsync(int bookingId, string? completionNotes)
    {
        // Using Stored Procedure sp_CompleteBooking
        // This automatically triggers trg_UpdateJobCompletionOnBooking (AFTER UPDATE trigger)
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("sp_CompleteBooking", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@BookingID", bookingId);
        command.Parameters.AddWithValue("@CompletionNotes", (object?)completionNotes ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
        return true;
    }

    public async Task<IEnumerable<object>> GetBookingSummaryByCategoryAsync()
    {
        // Using View vw_BookingSummaryByCategory
        var summary = new List<object>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("SELECT * FROM vw_BookingSummaryByCategory", connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            summary.Add(new
            {
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                ScheduledCount = reader.GetInt32(reader.GetOrdinal("ScheduledCount")),
                InProgressCount = reader.GetInt32(reader.GetOrdinal("InProgressCount")),
                CompletedCount = reader.GetInt32(reader.GetOrdinal("CompletedCount")),
                CancelledCount = reader.GetInt32(reader.GetOrdinal("CancelledCount")),
                TotalBookings = reader.GetInt32(reader.GetOrdinal("TotalBookings")),
                AverageCompletionRating = reader.IsDBNull(reader.GetOrdinal("AverageCompletionRating")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("AverageCompletionRating"))
            });
        }

        return summary;
    }

    private BookingDTO MapBookingFromReader(SqlDataReader reader)
    {
        return new BookingDTO
        {
            BookingID = reader.GetInt32(reader.GetOrdinal("BookingID")),
            JobID = reader.GetInt32(reader.GetOrdinal("JobID")),
            WorkerID = reader.GetInt32(reader.GetOrdinal("WorkerID")),
            BidID = reader.IsDBNull(reader.GetOrdinal("BidID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("BidID")),
            ScheduledStart = reader.IsDBNull(reader.GetOrdinal("ScheduledStart")) ? null : reader.GetDateTime(reader.GetOrdinal("ScheduledStart")),
            ScheduledEnd = reader.IsDBNull(reader.GetOrdinal("ScheduledEnd")) ? null : reader.GetDateTime(reader.GetOrdinal("ScheduledEnd")),
            ActualStart = reader.IsDBNull(reader.GetOrdinal("ActualStart")) ? null : reader.GetDateTime(reader.GetOrdinal("ActualStart")),
            ActualEnd = reader.IsDBNull(reader.GetOrdinal("ActualEnd")) ? null : reader.GetDateTime(reader.GetOrdinal("ActualEnd")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            CancellationReason = reader.IsDBNull(reader.GetOrdinal("CancellationReason")) ? null : reader.GetString(reader.GetOrdinal("CancellationReason")),
            BookingCode = reader.IsDBNull(reader.GetOrdinal("BookingCode")) ? null : reader.GetString(reader.GetOrdinal("BookingCode")),
            CompletionNotes = reader.IsDBNull(reader.GetOrdinal("CompletionNotes")) ? null : reader.GetString(reader.GetOrdinal("CompletionNotes")),
            JobTitle = reader.GetString(reader.GetOrdinal("JobTitle")),
            WorkerName = reader.GetString(reader.GetOrdinal("WorkerName")),
            CustomerName = reader.GetString(reader.GetOrdinal("CustomerName"))
        };
    }
}
