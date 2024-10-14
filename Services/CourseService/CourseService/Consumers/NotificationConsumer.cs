using CourseService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NotificationService.Configuration;
using NotificationService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace CourseService.Consumers;

public class NotificationConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConfiguration _rabbitMQSettings;

    public NotificationConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMQConfiguration> rabbitMQSettings)
    {
        _scopeFactory = scopeFactory;
        _rabbitMQSettings = rabbitMQSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _rabbitMQSettings.HostName,
            Port = _rabbitMQSettings.Port,
            UserName = _rabbitMQSettings.UserName,
            Password = _rabbitMQSettings.Password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // Declare the queue
        channel.QueueDeclare(queue: "notifications",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        // Handle messages from RabbitMQ
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            // Deserialize the notification message
            var notification = JsonSerializer.Deserialize<Notification>(message);

            // Log the received notification
            Console.WriteLine($"Received notification: {notification.Message}");

            // Process the notification
            await HandleNotificationAsync(notification);
        };

        // Start consuming the queue
        channel.BasicConsume(queue: "notifications",
                             autoAck: true,
                             consumer: consumer);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Handles the logic of processing the notification and enrolling the user in the default course.
    /// </summary>
    /// <param name="notification">Notification message received from RabbitMQ.</param>
    private async Task HandleNotificationAsync(Notification notification)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CourseDbContext>();

        // Extract the user ID from the notification message
        var userId = ExtractUserIdFromNotification(notification);

        // Log the extracted User ID
        if (userId != null)
        {
            Console.WriteLine($"Extracted User ID: {userId}");

            // Look for the default course in the database
            var defaultCourse = await dbContext.Courses.FirstOrDefaultAsync(c => c.IsDefaultCourse);

            if (defaultCourse != null)
            {
                Console.WriteLine($"Default course found: {defaultCourse.Title}");

                // Create the enrollment for the user
                var enrollment = new CourseEnrollment
                {
                    CourseId = defaultCourse.Id, // Use dynamic value
                    UserId = userId,              // Use extracted user ID
                    EnrollmentDate = DateTime.UtcNow
                };

                // Add the enrollment to the database
                dbContext.CourseEnrollments.Add(enrollment);
                await dbContext.SaveChangesAsync();

                // Log successful enrollment
                Console.WriteLine($"User {userId} enrolled in course {defaultCourse.Title}.");
            }
            else
            {
                // Log if no default course is found
                Console.WriteLine("No default course found for enrollment.");
            }
        }
        else
        {
            Console.WriteLine("User ID could not be extracted from the notification.");
        }
    }

    /// <summary>
    /// Extracts the user ID from the notification message.
    /// </summary>
    /// <param name="notification">Notification containing the user information.</param>
    /// <returns>The extracted user ID if found; otherwise, null.</returns>
    private string ExtractUserIdFromNotification(Notification notification)
    {
        try
        {
            // Assuming the message format contains the user ID in single quotes
            var userIdString = notification.Message.Split('\'')[1];
            return userIdString; // Return the extracted user ID
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting User ID: {ex.Message}");
            return null;
        }
    }
}
