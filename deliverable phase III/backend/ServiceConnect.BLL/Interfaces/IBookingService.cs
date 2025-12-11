using ServiceConnect.Models.DTOs;

namespace ServiceConnect.BLL.Interfaces;

public interface IBookingService
{
    // CRUD Operations
    Task<IEnumerable<BookingDTO>> GetAllBookingsAsync();
    Task<BookingDTO?> GetBookingByIdAsync(int bookingId);
    Task<BookingDTO> CreateBookingAsync(BookingDTO bookingDto);
    Task<bool> UpdateBookingAsync(BookingDTO bookingDto);
    Task<bool> DeleteBookingAsync(int bookingId);

    // Business Logic Operations
    Task<IEnumerable<BookingDTO>> GetBookingsByWorkerAsync(int workerId);
    Task<IEnumerable<BookingDTO>> GetBookingsByCustomerAsync(int customerId);

    // Using Stored Procedures
    Task<bool> CompleteBookingAsync(int bookingId, string? completionNotes);

    // Using Views
    Task<IEnumerable<object>> GetBookingSummaryByCategoryAsync();
}
