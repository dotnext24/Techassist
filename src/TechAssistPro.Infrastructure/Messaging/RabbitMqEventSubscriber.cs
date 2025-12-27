
using RabbitMQ.Client;
using System.Text;
using TechAssistPro.SharedKernel.Events;
using TechAssistPro.Infrastructure.SchemaRegistry;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace TechAssistPro.Infrastructure.Messaging
{
    public class RabbitMqEventSubscriber : IDisposable
    {
        private readonly IRabbitMQConnection _connection;
        private readonly ISchemaRegistry _schemaRegistry;

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMqEventSubscriber> _logger;

        private readonly List<IChannel> _channels = new();

        private readonly ActivitySource _activitySource;
        public RabbitMqEventSubscriber(IRabbitMQConnection connection, ISchemaRegistry schemaRegistry, IServiceProvider serviceProvider, ILogger<RabbitMqEventSubscriber> logger, ActivitySource activitySource)
        {
            _connection = connection;
            _schemaRegistry = schemaRegistry;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _activitySource = activitySource;
        }

        public async Task SubscribeAsync<TEvent>(
          string queueName,
          string exchangeName,
          int schemaVersion,
          string[] routingKeys,
          CancellationToken ct = default)
          where TEvent : class
        {

            using var activity = _activitySource.StartActivity("rabbitmq.consume", ActivityKind.Consumer);
            activity?.AddTag("rabbitmq.queue", queueName);
            activity?.AddTag("rabbitmq.deadletter.exchange", $"{exchangeName}.dlq");
            activity?.AddTag("rabbitmq.exchange", exchangeName);
            activity?.SetTag("schema.version", schemaVersion);

            _logger.LogInformation("Rabbitmq consumer - started. QueueName {queueName} | SchemaVersion {schemaVersion}", queueName, schemaVersion);


            var channel = await _connection.CreateChannelAsync();
            _channels.Add(channel);

            // Exchanges
            await channel.ExchangeDeclareAsync(
               exchangeName,
                ExchangeType.Topic,
                durable: true);

            // Queue
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    { "x-dead-letter-exchange", $"{exchangeName}.dlq" }
                });

            routingKeys = routingKeys.Select(k => $"{k}.v{schemaVersion}").ToArray();

            foreach (var routingKey in routingKeys)
            {
                await channel.QueueBindAsync(
                    queue: queueName,
                    exchange: exchangeName,
                    routingKey: routingKey);
            }

            _logger.LogInformation(
                 "🔌Rabbitmq consumer - Queue bound | Exchange={Exchange} | Queue={Queue} | RoutingKeys={RoutingKeys}",
                 exchangeName,
                 queueName,
                 string.Join(", ", routingKeys));


            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {

                    _logger.LogInformation(
                       "📥 Rabbitmq consumer - Message received | Exchange={Exchange} | RoutingKey={RoutingKey} | EventType={EventType} | DeliveryTag={DeliveryTag}",
                       ea.Exchange,
                       ea.RoutingKey,
                       ea.BasicProperties.Type,
                       ea.DeliveryTag);


                    var headers = ea.BasicProperties.Headers;
                    var schemaVersion = headers!.GetSchemaVersion();
                    var eventType = ea.BasicProperties.Type!;
                    var traceParent = headers!.GetTraceParent();
                    var correlationId = headers!.GetCorrelationId();

                    activity?.AddTag("trace-parent", traceParent);
                    activity?.AddTag("correlation-id", correlationId);

                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                    var message = JsonSerializer.Deserialize<TEvent>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        })!;

                    activity?.AddTag("message", message);


                    // 2. Validate against schema        
                    var isValid = await _schemaRegistry.ValidateAsync(
                        eventType,
                        schemaVersion,
                        json);

                    activity?.SetTag("schema.valid", isValid);


                    if (!isValid)
                    {
                        var errorMsg = $"Rabbitmq consumer - Event {eventType} v{schemaVersion} failed schema validation";

                        throw new InvalidOperationException(errorMsg);
                    }

                    using var scope = _serviceProvider.CreateScope();

                    var handler = scope.ServiceProvider
                        .GetRequiredService<IIntegrationEventHandler<TEvent>>();

                    await handler.HandleAsync(message, ct);

                    await channel.BasicAckAsync(ea.DeliveryTag, false);

                    _logger.LogInformation("📤 Rabbitmq consumer - message processed succesfully - IntegrationEvent {eventType}", eventType);

                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddException(ex);
                    _logger.LogError(ex,
                        "❌ Rabbitmq consumer - Error in processing {EventType} | Queue={Queue} | MessageId={MessageId}", typeof(TEvent).Name, queueName, ea.BasicProperties.MessageId);

                    await channel.BasicNackAsync(
                        ea.DeliveryTag,
                        multiple: false,
                        requeue: false);
                }
            };

            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation(
                "🐇 Rabbitmq consumer - Subscribed to {EventType} on {Queue}",
                typeof(TEvent).Name,
                queueName);
        }

        public void Dispose()
        {
            foreach (var channel in _channels)
            {
                channel.CloseAsync();
                channel.Dispose();
            }
        }
    }


}