using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceConnect.Models.Entities;

public class Bid
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BidID { get; set; }

    public int JobID { get; set; }

    public int WorkerID { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? BidAmount { get; set; }

    public DateTime? ProposedStartTime { get; set; }

    public int? EstimatedDuration { get; set; } // in minutes

    public string? CoverLetter { get; set; }

    public DateTime BidDate { get; set; } = DateTime.Now;

    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected, Withdrawn

    public bool IsWinningBid { get; set; } = false;

    // Navigation properties
    [ForeignKey("JobID")]
    public virtual Job Job { get; set; } = null!;

    [ForeignKey("WorkerID")]
    public virtual Worker Worker { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
