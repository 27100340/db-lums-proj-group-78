using System.ComponentModel.DataAnnotations;

namespace ServiceConnect.Models.DTOs;

public class BookingDTO
{
    public int? BookingID { get; set; }

    [Required(ErrorMessage = "Job ID is required")]
    public int JobID { get; set; }

    [Required(ErrorMessage = "Worker ID is required")]
    public int WorkerID { get; set; }

    public int? BidID { get; set; }

    public DateTime? ScheduledStart { get; set; }

    public DateTime? ScheduledEnd { get; set; }

    public DateTime? ActualStart { get; set; }

    public DateTime? ActualEnd { get; set; }

    public string? Status { get; set; }

    public string? CancellationReason { get; set; }

    public string? BookingCode { get; set; }

    public string? CompletionNotes { get; set; }

    //  display fields
    public string? JobTitle { get; set; }
    public string? WorkerName { get; set; }
    public string? CustomerName { get; set; }
}
