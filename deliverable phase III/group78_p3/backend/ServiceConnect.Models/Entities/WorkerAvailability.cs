using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceConnect.Models.Entities;

public class WorkerAvailability
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AvailabilityID { get; set; }

    public int WorkerID { get; set; }

    [Range(1, 7)]
    public int DayOfWeek { get; set; } // 1 = Monday, 7 = Sunday

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public bool IsRecurring { get; set; } = true;

    // navigation
    [ForeignKey("WorkerID")]
    public virtual Worker Worker { get; set; } = null!;
}
