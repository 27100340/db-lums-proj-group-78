using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceConnect.Models.Entities;

public class WorkerSkill
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SkillID { get; set; }

    public int WorkerID { get; set; }

    public int CategoryID { get; set; }

    [StringLength(20)]
    public string? SkillLevel { get; set; } // beginner intermediate expert

    [StringLength(255)]
    public string? CertificationURL { get; set; }

    public DateTime? CertificationExpiry { get; set; }

    public int? YearsExperience { get; set; }

    // navigation
    [ForeignKey("WorkerID")]
    public virtual Worker Worker { get; set; } = null!;

    [ForeignKey("CategoryID")]
    public virtual ServiceCategory Category { get; set; } = null!;
}
