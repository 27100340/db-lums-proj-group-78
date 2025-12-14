using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceConnect.Models.Entities;

public class Booking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BookingID { get; set; }

    public int JobID { get; set; }

    public int WorkerID { get; set; }

    public int BidID { get; set; }

    public DateTime? ScheduledStart { get; set; }

    public DateTime? ScheduledEnd { get; set; }

    public DateTime? ActualStart { get; set; }

    public DateTime? ActualEnd { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed, Cancelled

    [StringLength(255)]
    public string? CancellationReason { get; set; }

    [StringLength(20)]
    public string? BookingCode { get; set; }

    public string? CompletionNotes { get; set; }

    // navigation
    [ForeignKey("JobID")]
    public virtual Job Job { get; set; } = null!;

    [ForeignKey("WorkerID")]
    public virtual Worker Worker { get; set; } = null!;

    [ForeignKey("BidID")]
    public virtual Bid Bid { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
