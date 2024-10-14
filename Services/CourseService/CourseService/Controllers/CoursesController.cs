using CourseService.Models;
using CourseService.Services;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.Services;

namespace CourseService.Controllers;

[ApiController]
[Route("[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly NotificationPublisher _notificationPublisher;

    public CoursesController(ICourseService courseService, NotificationPublisher notificationPublisher)
    {
        _courseService = courseService;
        _notificationPublisher = notificationPublisher;
    }

    [HttpPost]
    public async Task<ActionResult<Course>> CreateCourse(Course course)
    {
        var createdCourse = await _courseService.CreateCourseAsync(course);
        // Send notification
        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            Message = $"Course '{createdCourse.Id}' created successfully.",
            CreatedAt = DateTime.UtcNow
        };
        _notificationPublisher.Publish(notification);
        return CreatedAtAction(nameof(GetCourseById), new { id = createdCourse.Id }, createdCourse);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Course>>> GetAllCourses()
    {
        var courses = await _courseService.GetAllCoursesAsync();
        return Ok(courses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Course>> GetCourseById(int id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        return course == null ? (ActionResult<Course>)NotFound() : (ActionResult<Course>)Ok(course);
    }

    [HttpPut]
    public async Task<ActionResult<Course>> UpdateCourse(Course course)
    {
        var updatedCourse = await _courseService.UpdateCourseAsync(course);
        return updatedCourse == null ? (ActionResult<Course>)NotFound() : (ActionResult<Course>)Ok(updatedCourse);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var deleted = await _courseService.DeleteCourseAsync(id);
        return !deleted ? NotFound() : NoContent();
    }
}
