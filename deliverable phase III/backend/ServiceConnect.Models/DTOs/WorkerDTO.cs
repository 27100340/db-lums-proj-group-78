using System.ComponentModel.DataAnnotations;

namespace ServiceConnect.Models.DTOs;

public class WorkerDTO
{
    public int? WorkerID { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string? Address { get; set; }

    [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
    public string? City { get; set; }

    public string? PostalCode { get; set; }

    [Range(0, 1000, ErrorMessage = "Hourly rate must be between 0 and 1000")]
    public decimal? HourlyRate { get; set; }

    public decimal? OverallRating { get; set; }
    public int TotalJobsCompleted { get; set; }
    public string? Bio { get; set; }

    // For registration only
    public string? Password { get; set; }
}
