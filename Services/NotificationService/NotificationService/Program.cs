using NotificationService.Configuration;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure RabbitMQ settings
builder.Services.Configure<RabbitMQConfiguration>(
    builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddSingleton<NotificationPublisher>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
