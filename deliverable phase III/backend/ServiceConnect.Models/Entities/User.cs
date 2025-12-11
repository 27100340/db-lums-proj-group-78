using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceConnect.Models.Entities;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserID { get; set; }

    [Required]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    [StringLength(20)]
    public string UserType { get; set; } = string.Empty; // Customer or Worker

    public DateTime RegistrationDate { get; set; } = DateTime.Now;

    public DateTime? LastActive { get; set; }

    public bool IsVerified { get; set; } = false;

    [StringLength(20)]
    public string AccountStatus { get; set; } = "Active";

    // Navigation properties
    public virtual Worker? Worker { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
