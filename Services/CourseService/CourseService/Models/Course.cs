using System.ComponentModel.DataAnnotations;

namespace CourseService.Models;

public class Course
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; }

    [Required]
    [StringLength(100)]
    public string Instructor { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // New field to identify the default course for user enrollment
    [Required]
    public bool IsDefaultCourse { get; set; } = false;
    // New relationship to course enrollments
    public ICollection<CourseEnrollment>? Enrollments { get; set; }
}