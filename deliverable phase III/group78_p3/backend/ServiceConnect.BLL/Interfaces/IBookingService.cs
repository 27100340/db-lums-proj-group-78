using ServiceConnect.Models.DTOs;

namespace ServiceConnect.BLL.Interfaces;

public interface IBookingService
{
    // create, read, update, delete
    Task<IEnumerable<BookingDTO>> GetAllBookingsAsync();
    Task<BookingDTO?> GetBookingByIdAsync(int bookingId);
    Task<BookingDTO> CreateBookingAsync(BookingDTO bookingDto);
    Task<bool> UpdateBookingAsync(BookingDTO bookingDto);
    Task<bool> DeleteBookingAsync(int bookingId);

    // logic operations
    Task<IEnumerable<BookingDTO>> GetBookingsByWorkerAsync(int workerId);
    Task<IEnumerable<BookingDTO>> GetBookingsByCustomerAsync(int customerId);

    // sprocs
    Task<bool> CompleteBookingAsync(int bookingId, string? completionNotes);

    // views
    Task<IEnumerable<object>> GetBookingSummaryByCategoryAsync();
}
