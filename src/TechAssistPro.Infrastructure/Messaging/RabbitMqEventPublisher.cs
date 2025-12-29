
using RabbitMQ.Client;
using System.Text;
using TechAssistPro.SharedKernel.Events;
using TechAssistPro.Infrastructure.SchemaRegistry;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MediatR;
using TechAssistPro.Infrastructure.Observability;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace TechAssistPro.Infrastructure.Messaging
{
    public class RabbitMqEventPublisher
        : IEventPublisher,
          IAsyncDisposable, IDisposable
    {
        private readonly IRabbitMQConnection _connection;
        private readonly ISchemaRegistry _schemaRegistry;
        private IChannel? _channel;
        private readonly ILogger<RabbitMqEventPublisher> _logger;
        private readonly MessagingOptions _options;
        private readonly ActivitySource _activitySource;
        public RabbitMqEventPublisher(IRabbitMQConnection connection, ISchemaRegistry schemaRegistry, IOptions<MessagingOptions> options, ILogger<RabbitMqEventPublisher> logger, ActivitySource activitySource)
        {
            _connection = connection;
            _schemaRegistry = schemaRegistry;
            _logger = logger;
            _options = options.Value;
            _activitySource = activitySource;
        }


        public async Task PublishAsync(
            string eventType,
            object eventData,
            int schemaVersion,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var activity = _activitySource.StartActivity("rabbitmq.publish", ActivityKind.Producer);
            activity?.SetTag("event.type", eventType);
            activity?.SetTag("schema.version", schemaVersion);
            activity?.SetTag("correlation.id", CorrelationContext.CorrelationId);


            _logger.LogInformation("Rabbitmq publisher -  started. EventType {eventType} | SchemaVersion {schemaVersion}", eventType, schemaVersion);
            // 1. Serialize event data
            string payload = JsonSerializer.Serialize(eventData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            _logger.LogInformation($"Rabbitmq publisher - Payload of {eventType}: {payload}");

            // 2. Validate against schema        
            var isValid = await _schemaRegistry.ValidateAsync(
                eventType,
                schemaVersion,
                payload);

            activity?.SetTag("schema.valid", isValid);

            if (!isValid)
            {
                var errorMsg = $"Rabbitmq publisher - Event {eventType} v{schemaVersion} failed schema validation";

                throw new InvalidOperationException(errorMsg);
            }

            _channel = await _connection.CreateChannelAsync();
            // 3. ExchangeDeclare

            await _channel.ExchangeDeclareAsync(_options.ExchangeName, ExchangeType.Topic, durable: true);

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
                { "published-at", DateTime.UtcNow.ToString("O") },
                { "traceparent", CorrelationContext.CorrelationId},
                { "correlation.id", CorrelationContext.CorrelationId}
            }
            };

            _logger.LogInformation("📤 Rabbitmq publisher - Publishing - IntegrationEvent {eventType} | Exchange={Exchange} | RoutingKey={RoutingKey} | EventType={EventType} | SchemaVersion={SchemaVersion} | MessageId={MessageId}",
                  eventType,
                  _options.ExchangeName,
                  $"{eventType}.v{schemaVersion}",
                  eventType,
                  schemaVersion,
                  eventId);

            await _channel.BasicPublishAsync(
                 exchange: _options.ExchangeName,
                 routingKey: $"{eventType}.v{schemaVersion}",
                 mandatory: false,
                 basicProperties: properties,
                 body: body);

            _logger.LogInformation("✅ Rabbitmq publisher - Published - IntegrationEvent {eventType} | Exchange={Exchange} | RoutingKey={RoutingKey} | MessageId={MessageId}",
                 eventType,
                 _options.ExchangeName,
                 $"{eventType}.v{schemaVersion}",
                 eventId);

            activity?.SetStatus(ActivityStatusCode.Ok);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }
        }

        public void Dispose()
        => DisposeAsync().AsTask().GetAwaiter().GetResult();


    }


}