using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceConnect.Models.Entities;

public class Job
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int JobID { get; set; }

    public int CustomerID { get; set; }

    public int CategoryID { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? Budget { get; set; }

    public DateTime PostedDate { get; set; } = DateTime.Now;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [Column(TypeName = "decimal(10,7)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(10,7)")]
    public decimal? Longitude { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Open"; // Open, Assigned, InProgress, Completed, Cancelled

    [StringLength(20)]
    public string? UrgencyLevel { get; set; } // Low, Medium, High, Urgent

    public int RequiredWorkers { get; set; } = 1;

    public int CompletedWorkers { get; set; } = 0;

    // Navigation properties
    [ForeignKey("CustomerID")]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("CategoryID")]
    public virtual ServiceCategory Category { get; set; } = null!;

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<JobAttachment> Attachments { get; set; } = new List<JobAttachment>();
}
