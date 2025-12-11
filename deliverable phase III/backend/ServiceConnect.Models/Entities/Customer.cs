using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceConnect.Models.Entities;

public class Customer
{
    [Key]
    [ForeignKey("User")]
    public int CustomerID { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(50)]
    public string? City { get; set; }

    [StringLength(10)]
    public string? PostalCode { get; set; }

    [StringLength(255)]
    public string? ProfilePhotoURL { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal? CustomerRating { get; set; }

    public int TotalJobsPosted { get; set; } = 0;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
