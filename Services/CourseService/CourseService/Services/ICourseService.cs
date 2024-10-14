using CourseService.Models;

namespace CourseService.Services;

public interface ICourseService
{
    Task<Course> CreateCourseAsync(Course course);
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task<Course> GetCourseByIdAsync(int id);
    Task<Course> UpdateCourseAsync(Course course);
    Task<bool> DeleteCourseAsync(int id);
}