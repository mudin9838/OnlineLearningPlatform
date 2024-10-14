using NotificationService.Configuration;
using NotificationService.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace NotificationService.Services;

using Microsoft.Extensions.Options; // Add this using directive

public class NotificationPublisher
{
    private readonly RabbitMQConfiguration _rabbitMQSettings;

    public NotificationPublisher(IOptions<RabbitMQConfiguration> rabbitMQSettings)
    {
        _rabbitMQSettings = rabbitMQSettings.Value;
    }

    public void Publish(Notification notification)
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

        channel.QueueDeclare(queue: "notifications",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var message = JsonSerializer.Serialize(notification);
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "",
                             routingKey: "notifications",
                             basicProperties: null,
                             body: body);
    }
}