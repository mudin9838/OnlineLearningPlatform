


// Services/CourseService.cs
using global::CourseService.Models;
using Microsoft.EntityFrameworkCore;
namespace CourseService.Services;

public class CoursesService : ICourseService
{
    private readonly CourseDbContext _context;

    public CoursesService(CourseDbContext context)
    {
        _context = context;
    }


    public async Task<Course> CreateCourseAsync(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        return await _context.Courses.ToListAsync();
    }

    public Task<Course> GetCourseByIdAsync(int id)
    {
        var course = _context.Courses.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(course);
    }

    public Task<Course> UpdateCourseAsync(Course course)
    {
        var existingCourse = _context.Courses.FirstOrDefault(c => c.Id == course.Id);
        if (existingCourse != null)
        {
            existingCourse.Title = course.Title;
            existingCourse.Description = course.Description;
            existingCourse.Instructor = course.Instructor;
            existingCourse.UpdatedAt = DateTime.UtcNow;
        }
        return Task.FromResult(existingCourse);
    }

    public Task<bool> DeleteCourseAsync(int id)
    {
        var course = _context.Courses.FirstOrDefault(c => c.Id == id);
        if (course != null)
        {
            _context.Remove(course);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}

