using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.Services;

namespace NotificationService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly NotificationPublisher _notificationPublisher;

    public NotificationController(NotificationPublisher notificationPublisher)
    {
        _notificationPublisher = notificationPublisher;
    }

    [HttpPost]
    public IActionResult SendNotification([FromBody] Notification notification)
    {
        notification.Id = Guid.NewGuid().ToString();
        notification.CreatedAt = DateTime.UtcNow;

        _notificationPublisher.Publish(notification);

        return Ok(notification);
    }
}
