using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseService.Models;

public class CourseEnrollment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CourseId { get; set; }

    [ForeignKey("CourseId")]
    public Course Course { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    // Optional: If you need to store additional enrollment details
    public bool IsActive { get; set; } = true;
}

