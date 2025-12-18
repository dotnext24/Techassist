
using RabbitMQ.Client;
using System.Text;
using TechAssistPro.SharedKernel.Events;
using TechAssistPro.Infrastructure.SchemaRegistry;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MediatR;
using TechAssistPro.Infrastructure.Events;

namespace TechAssistPro.Infrastructure.Messaging
{
    public class RabbitMqEventPublisher 
        : IEventPublisher,
          IAsyncDisposable
    {
        private readonly IRabbitMQConnection _connection;
        private readonly ISchemaRegistry _schemaRegistry;
        private IChannel? _channel;
        private readonly ILogger<RabbitMqEventPublisher> _logger;
        private readonly SemaphoreSlim _channelLock = new(1, 1);
        public RabbitMqEventPublisher(IRabbitMQConnection connection, ISchemaRegistry schemaRegistry, ILogger<RabbitMqEventPublisher> logger)
        {
            _connection = connection;
            _schemaRegistry = schemaRegistry;
            _logger = logger;
        }
      

        public async Task PublishAsync(
            string eventType,
            object eventData,
            int schemaVersion,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("RabbitMqEventPublisher Called");
            // 1. Serialize event data
            string payload = JsonSerializer.Serialize(eventData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            // 2. Validate against schema        
            var isValid = await _schemaRegistry.ValidateAsync(
                eventType,
                schemaVersion,
                payload);

            if (!isValid)
            {
                var errorMsg = $"Event {eventType} v{schemaVersion} failed schema validation";

                throw new InvalidOperationException(errorMsg);
            }

            _channel = await _connection.CreateChannelAsync();
            // 3. ExchangeDeclare

            await _channel.ExchangeDeclareAsync("ticket.events", ExchangeType.Topic, durable: true);

            // 4. Publish to RabbitMQ
            var body = Encoding.UTF8.GetBytes(payload);

            var eventId = Guid.NewGuid();
            var properties = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = eventId.ToString(),
                Type = eventType,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Headers = new Dictionary<string, object?>
            {
                { "event-type", eventType },
                { "schema-version", schemaVersion },
                { "schema-validated", true },
                { "published-at", DateTime.UtcNow.ToString("O") }
            }
            };

            await _channel.BasicPublishAsync(
                 exchange: "ticket.events",
                 routingKey: $"{eventType}.v{schemaVersion}",
                 mandatory: false,
                 basicProperties: properties,
                 body: body);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }
        }
    }


}