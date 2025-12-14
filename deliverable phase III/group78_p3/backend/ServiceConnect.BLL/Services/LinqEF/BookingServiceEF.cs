using Microsoft.EntityFrameworkCore;
using ServiceConnect.BLL.Data;
using ServiceConnect.BLL.Interfaces;
using ServiceConnect.Models.DTOs;
using ServiceConnect.Models.Entities;

namespace ServiceConnect.BLL.Services.LinqEF;

public class BookingServiceEF : IBookingService
{
    private readonly ServiceConnectDbContext _context;

    public BookingServiceEF(ServiceConnectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BookingDTO>> GetAllBookingsAsync()
    {
        return await _context.Bookings
            .Include(b => b.Job)
            .ThenInclude(j => j.Customer)
            .Include(b => b.Worker)
            .Select(b => new BookingDTO
            {
                BookingID = b.BookingID,
                JobID = b.JobID,
                WorkerID = b.WorkerID,
                BidID = b.BidID,
                ScheduledStart = b.ScheduledStart,
                ScheduledEnd = b.ScheduledEnd,
                ActualStart = b.ActualStart,
                ActualEnd = b.ActualEnd,
                Status = b.Status,
                CancellationReason = b.CancellationReason,
                BookingCode = b.BookingCode,
                CompletionNotes = b.CompletionNotes,
                JobTitle = b.Job.Title,
                WorkerName = b.Worker.FirstName + " " + b.Worker.LastName,
                CustomerName = b.Job.Customer.FirstName + " " + b.Job.Customer.LastName
            })
            .OrderByDescending(b => b.ScheduledStart)
            .ThenByDescending(b => b.ScheduledEnd)
            .ThenByDescending(b => b.BookingID)
            .Take(100)
            .ToListAsync();
    }

    public async Task<BookingDTO?> GetBookingByIdAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Job)
            .ThenInclude(j => j.Customer)
            .Include(b => b.Worker)
            .FirstOrDefaultAsync(b => b.BookingID == bookingId);

        if (booking == null) return null;

        return new BookingDTO
        {
            BookingID = booking.BookingID,
            JobID = booking.JobID,
            WorkerID = booking.WorkerID,
            BidID = booking.BidID,
            ScheduledStart = booking.ScheduledStart,
            ScheduledEnd = booking.ScheduledEnd,
            ActualStart = booking.ActualStart,
            ActualEnd = booking.ActualEnd,
            Status = booking.Status,
            CancellationReason = booking.CancellationReason,
            BookingCode = booking.BookingCode,
            CompletionNotes = booking.CompletionNotes,
            JobTitle = booking.Job.Title,
            WorkerName = booking.Worker.FirstName + " " + booking.Worker.LastName,
            CustomerName = booking.Job.Customer.FirstName + " " + booking.Job.Customer.LastName
        };
    }

    public async Task<BookingDTO> CreateBookingAsync(BookingDTO bookingDto)
    {
        var booking = new Booking
        {
            JobID = bookingDto.JobID,
            WorkerID = bookingDto.WorkerID,
            BidID = bookingDto.BidID.GetValueOrDefault(),
            ScheduledStart = bookingDto.ScheduledStart,
            ScheduledEnd = bookingDto.ScheduledEnd,
            Status = "Scheduled",
            BookingCode = "BK" + new Random().Next(100000, 999999).ToString()
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        bookingDto.BookingID = booking.BookingID;
        bookingDto.BookingCode = booking.BookingCode;
        bookingDto.Status = booking.Status;

        return bookingDto;
    }

    public async Task<bool> UpdateBookingAsync(BookingDTO bookingDto)
    {
        var booking = await _context.Bookings.FindAsync(bookingDto.BookingID);
        if (booking == null) return false;

        booking.ScheduledStart = bookingDto.ScheduledStart;
        booking.ScheduledEnd = bookingDto.ScheduledEnd;
        booking.ActualStart = bookingDto.ActualStart;
        booking.ActualEnd = bookingDto.ActualEnd;
        booking.Status = bookingDto.Status ?? booking.Status;
        booking.CancellationReason = bookingDto.CancellationReason;
        booking.CompletionNotes = bookingDto.CompletionNotes;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBookingAsync(int bookingId)
    {
        // showing trg_PreventDeleteCompletedBooking in place of delete trigger
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null) return false;

        if (booking.Status == "Completed")
        {
            throw new InvalidOperationException("Cannot delete completed bookings for audit purposes");
        }

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<BookingDTO>> GetBookingsByWorkerAsync(int workerId)
    {
        return await _context.Bookings
            .Include(b => b.Job)
            .ThenInclude(j => j.Customer)
            .Where(b => b.WorkerID == workerId)
            .Select(b => new BookingDTO
            {
                BookingID = b.BookingID,
                JobID = b.JobID,
                WorkerID = b.WorkerID,
                ScheduledStart = b.ScheduledStart,
                ScheduledEnd = b.ScheduledEnd,
                Status = b.Status,
                BookingCode = b.BookingCode,
                JobTitle = b.Job.Title,
                CustomerName = b.Job.Customer.FirstName + " " + b.Job.Customer.LastName
            })
            .OrderByDescending(b => b.ScheduledStart)
            .ToListAsync();
    }

    public async Task<IEnumerable<BookingDTO>> GetBookingsByCustomerAsync(int customerId)
    {
        return await _context.Bookings
            .Include(b => b.Job)
            .Include(b => b.Worker)
            .Where(b => b.Job.CustomerID == customerId)
            .Select(b => new BookingDTO
            {
                BookingID = b.BookingID,
                JobID = b.JobID,
                WorkerID = b.WorkerID,
                ScheduledStart = b.ScheduledStart,
                ScheduledEnd = b.ScheduledEnd,
                Status = b.Status,
                BookingCode = b.BookingCode,
                JobTitle = b.Job.Title,
                WorkerName = b.Worker.FirstName + " " + b.Worker.LastName
            })
            .OrderByDescending(b => b.ScheduledStart)
            .ToListAsync();
    }

    public async Task<bool> CompleteBookingAsync(int bookingId, string? completionNotes)
    {
        // simulating sp_CompleteBooking stored procedure
        // this should trigger trg_UpdateJobCompletionOnBooking 
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Job)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId);

            if (booking == null) return false;

            // change booking
            booking.Status = "Completed";
            booking.ActualEnd = DateTime.Now;
            booking.CompletionNotes = completionNotes;

            // modify job status
            booking.Job.Status = "Completed";

            await _context.SaveChangesAsync();

            // trigger trg_UpdateJobCompletionOnBooking
            var completedCount = await _context.Bookings
                .CountAsync(b => b.JobID == booking.JobID && b.Status == "Completed");

            booking.Job.CompletedWorkers = completedCount;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<object>> GetBookingSummaryByCategoryAsync()
    {
        // using view query vw_BookingSummaryByCategory
        var summary = await (from b in _context.Bookings
                             join j in _context.Jobs on b.JobID equals j.JobID
                             join sc in _context.ServiceCategories on j.CategoryID equals sc.CategoryID
                             group new { b, j } by sc.CategoryName into g
                             select new
                             {
                                 CategoryName = g.Key,
                                 ScheduledCount = g.Count(x => x.b.Status == "Scheduled"),
                                 InProgressCount = g.Count(x => x.b.Status == "InProgress"),
                                 CompletedCount = g.Count(x => x.b.Status == "Completed"),
                                 CancelledCount = g.Count(x => x.b.Status == "Cancelled"),
                                 TotalBookings = g.Count(),
                                 AverageCompletionRating = _context.Reviews
                                     .Where(r => g.Select(x => x.b.BookingID).Contains(r.BookingID))
                                     .Average(r => (double?)r.Rating) ?? 0
                             }).ToListAsync();

        return summary;
    }
}
