using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceConnect.Models.Entities;

public class Notification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int NotificationID { get; set; }

    public int UserID { get; set; }

    [StringLength(50)]
    public string? NotificationType { get; set; }

    [StringLength(100)]
    public string? Title { get; set; }

    public string? Message { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public bool IsRead { get; set; } = false;

    public int? RelatedEntityID { get; set; }

    [StringLength(50)]
    public string? RelatedEntityType { get; set; }

    // Navigation properties
    [ForeignKey("UserID")]
    public virtual User User { get; set; } = null!;
}
