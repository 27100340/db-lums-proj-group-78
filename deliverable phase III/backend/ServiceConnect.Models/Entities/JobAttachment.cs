using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceConnect.Models.Entities;

public class JobAttachment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AttachmentID { get; set; }

    public int JobID { get; set; }

    [StringLength(255)]
    public string? FileURL { get; set; }

    [StringLength(50)]
    public string? FileType { get; set; }

    public DateTime UploadedDate { get; set; } = DateTime.Now;

    [StringLength(255)]
    public string? Description { get; set; }

    // navigation
    [ForeignKey("JobID")]
    public virtual Job Job { get; set; } = null!;
}
