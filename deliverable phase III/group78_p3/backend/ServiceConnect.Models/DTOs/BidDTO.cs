using System.ComponentModel.DataAnnotations;

namespace ServiceConnect.Models.DTOs;

public class BidDTO
{
    public int? BidID { get; set; }

    [Required(ErrorMessage = "Job ID is required")]
    public int JobID { get; set; }

    [Required(ErrorMessage = "Worker ID is required")]
    public int WorkerID { get; set; }

    [Required(ErrorMessage = "Bid amount is required")]
    [Range(1, 1000000, ErrorMessage = "Bid amount must be between 1 and 1,000,000")]
    public decimal BidAmount { get; set; }

    public DateTime? ProposedStartTime { get; set; }

    [Range(30, 10080, ErrorMessage = "Duration must be between 30 minutes and 1 week")]
    public int? EstimatedDuration { get; set; }

    [StringLength(1000, ErrorMessage = "Cover letter cannot exceed 1000 characters")]
    public string? CoverLetter { get; set; }

    public string? Status { get; set; }
    public bool IsWinningBid { get; set; }
    public DateTime? BidDate { get; set; }

    // Additional fields for display
    public string? WorkerName { get; set; }
    public string? JobTitle { get; set; }
}
