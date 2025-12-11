using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceConnect.Models.Entities;

public class Review
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ReviewID { get; set; }

    public int BookingID { get; set; }

    public int ReviewerID { get; set; }

    public int ReviewedID { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime ReviewDate { get; set; } = DateTime.Now;

    public bool IsDisputed { get; set; } = false;

    public string? DisputeResolution { get; set; }

    public int WasHelpful { get; set; } = 0;

    // Navigation properties
    [ForeignKey("BookingID")]
    public virtual Booking Booking { get; set; } = null!;

    [ForeignKey("ReviewerID")]
    public virtual User Reviewer { get; set; } = null!;

    [ForeignKey("ReviewedID")]
    public virtual User Reviewed { get; set; } = null!;
}
