using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceConnect.Models.Entities;

public class ServiceCategory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CategoryID { get; set; }

    [Required]
    [StringLength(50)]
    public string CategoryName { get; set; } = string.Empty;

    public string? CategoryDescription { get; set; }

    [StringLength(255)]
    public string? IconURL { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? BaseRate { get; set; }

    public bool IsActive { get; set; } = true;

    // navigation
    public virtual ICollection<WorkerSkill> WorkerSkills { get; set; } = new List<WorkerSkill>();
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
