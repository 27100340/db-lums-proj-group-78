using System.ComponentModel.DataAnnotations;

namespace ServiceConnect.Models.DTOs;

public class JobDTO
{
    public int? JobID { get; set; }

    [Required(ErrorMessage = "Customer ID is required")]
    public int CustomerID { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public int CategoryID { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, 1000000, ErrorMessage = "Budget must be between 0 and 1,000,000")]
    public decimal? Budget { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
    public string? Location { get; set; }

    public string? Status { get; set; }

    public string? UrgencyLevel { get; set; }

    [Range(1, 100, ErrorMessage = "Required workers must be between 1 and 100")]
    public int RequiredWorkers { get; set; } = 1;

    // Additional fields for display
    public string? CustomerName { get; set; }
    public string? CategoryName { get; set; }
    public DateTime? PostedDate { get; set; }
    public int? TotalBids { get; set; }
}
