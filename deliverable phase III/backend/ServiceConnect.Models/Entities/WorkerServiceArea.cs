using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceConnect.Models.Entities;

public class WorkerServiceArea
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ServiceAreaID { get; set; }

    public int WorkerID { get; set; }

    [StringLength(50)]
    public string? City { get; set; }

    [StringLength(10)]
    public string? PostalCode { get; set; }

    public int? MaxDistance { get; set; } // in kilometers

    public bool IsActive { get; set; } = true;

    // Navigation properties
    [ForeignKey("WorkerID")]
    public virtual Worker Worker { get; set; } = null!;
}
