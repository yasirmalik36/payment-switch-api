using Microsoft.Extensions.Logging;
using PaymentSwitch.Application.Interfaces;
using System.Text.Json;
namespace PaymentSwitch.Infrastructure.Messaging;
/// <summary>
/// In-memory message publisher for demo.
/// Replace with RabbitMQ publisher for production (see docs/rabbitmq-setup.md)
/// </summary>
public class InMemoryMessagePublisher : IMessagePublisher
{
    private readonly ILogger<InMemoryMessagePublisher> _logger;
    public InMemoryMessagePublisher(ILogger<InMemoryMessagePublisher> logger) => _logger = logger;
    public Task PublishAsync<T>(string routingKey, T message, CancellationToken ct = default) where T : class
    {
        _logger.LogInformation("[MSG] routing_key={RoutingKey} payload={Payload}", routingKey, JsonSerializer.Serialize(message));
        return Task.CompletedTask;
    }
}