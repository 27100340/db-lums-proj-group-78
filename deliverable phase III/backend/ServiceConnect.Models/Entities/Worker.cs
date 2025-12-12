using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceConnect.Models.Entities;

public class Worker
{
    [Key]
    [ForeignKey("User")]
    public int WorkerID { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    [StringLength(50)]
    public string? GovernmentID { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(50)]
    public string? City { get; set; }

    [StringLength(10)]
    public string? PostalCode { get; set; }

    [StringLength(20)]
    public string? BackgroundCheckStatus { get; set; }

    public DateTime? BackgroundCheckDate { get; set; }

    [StringLength(50)]
    public string? InsuranceNumber { get; set; }

    [StringLength(255)]
    public string? ProfilePhotoURL { get; set; }

    public string? Bio { get; set; }

    public int? ExperienceYears { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? HourlyRate { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal? OverallRating { get; set; }

    public int TotalJobsCompleted { get; set; } = 0;

    // navigation
    public virtual User User { get; set; } = null!;
    public virtual ICollection<WorkerSkill> WorkerSkills { get; set; } = new List<WorkerSkill>();
    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<WorkerAvailability> Availabilities { get; set; } = new List<WorkerAvailability>();
    public virtual ICollection<WorkerServiceArea> ServiceAreas { get; set; } = new List<WorkerServiceArea>();
}
