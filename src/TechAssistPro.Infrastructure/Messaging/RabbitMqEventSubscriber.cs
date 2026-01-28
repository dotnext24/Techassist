
using RabbitMQ.Client;
using System.Text;
using TechAssistPro.SharedKernel.Events;
using TechAssistPro.Infrastructure.SchemaRegistry;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using TechAssistPro.Infrastructure.Observability;
using TechAssistPro.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

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

        public const int _maxRetryCount = 3;
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
            _logger.LogInformation("Rabbitmq consumer started | Exchange={Exchange} | Queue={Queue}",
                 exchangeName,
                 queueName);

            var channel = await _connection.CreateChannelAsync();
            _channels.Add(channel);

            await channel.BasicQosAsync(0, prefetchCount: 10, global: false);

            await DeclareTopology(channel, exchangeName, queueName);

            foreach (var key in routingKeys.Select(k => $"{k}.v{schemaVersion}"))
            {
                await channel.QueueBindAsync(queueName, exchangeName, key);
            }

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) => await ProcessMessage<TEvent>(channel, ea, exchangeName, queueName, schemaVersion, ct);

            await channel.BasicConsumeAsync(queueName, autoAck: false, consumer);

            _logger.LogInformation(
               "🐇 Rabbitmq consumer - Subscribed to {EventType} - Queue bound | Exchange={Exchange} | Queue={Queue} | RoutingKeys={RoutingKeys}",
               typeof(TEvent).Name,
               exchangeName,
               queueName,
               string.Join(", ", routingKeys));
        }


        private async Task DeclareTopology(IChannel channel, string exchangeName, string queueName)
        {
            // 1️⃣ Main exchange (producer-owned contract)
            await channel.ExchangeDeclareAsync(
                 exchange: exchangeName,
                 type: ExchangeType.Topic,
                 durable: true,
                 autoDelete: false);

            // 2️⃣ Retry exchange (infrastructure-owned)
            await channel.ExchangeDeclareAsync(
                exchange: $"{exchangeName}.retry",
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            // 3️⃣ DLQ exchange (terminal sink)
            await channel.ExchangeDeclareAsync(
                 exchange: $"{exchangeName}.dlq",
                 type: ExchangeType.Fanout,
                 durable: true,
                 autoDelete: false);

            // 4️⃣ Main queue
            await channel.QueueDeclareAsync(
                 queue: queueName,
                 durable: true,
                 exclusive: false,
                 autoDelete: false,
                 arguments: new Dictionary<string, object?>
                 {
                     // Dead-letter to retry exchange
                     ["x-dead-letter-exchange"] = $"{exchangeName}.retry",
                     ["x-dead-letter-routing-key"] = "retry.1m"
                 });

            // 5️⃣ Retry 1 minute
            await channel.QueueDeclareAsync(
                queue: $"{queueName}.retry.1m",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    ["x-message-ttl"] = 60_000,
                    ["x-dead-letter-exchange"] = exchangeName,
                    ["x-dead-letter-routing-key"] = queueName
                });

            await channel.QueueBindAsync(
                queue: $"{queueName}.retry.1m",
                exchange: $"{exchangeName}.retry",
                routingKey: "retry.1m");

            // 6️⃣ Retry 5 minutes
            await channel.QueueDeclareAsync(
               queue: $"{queueName}.retry.5m",
               durable: true,
               exclusive: false,
               autoDelete: false,
               arguments: new Dictionary<string, object?>
               {
                   ["x-message-ttl"] = 300_000,
                   ["x-dead-letter-exchange"] = exchangeName,
                   ["x-dead-letter-routing-key"] = queueName
               });

            await channel.QueueBindAsync(
                queue: $"{queueName}.retry.5m",
                exchange: $"{exchangeName}.retry",
                routingKey: "retry.5m");

            // 7️⃣ DLQ (no dead-lettering from here)
            await channel.QueueDeclareAsync(
                 queue: $"{queueName}.dlq",
                 durable: true,
                 exclusive: false,
                 autoDelete: false);

            await channel.QueueBindAsync(
                queue: $"{queueName}.dlq",
                exchange: $"{exchangeName}.dlq",
                routingKey: string.Empty);
        }


        private async Task ProcessMessage<TEvent>(IChannel channel, BasicDeliverEventArgs ea, string exchangeName, string queueName, int expectedSchemaVersion, CancellationToken ct)
        {
            using var activity = _activitySource.StartActivity(
                "rabbitmq.consume",
                ActivityKind.Consumer);

            try
            {

                var headers = ea.BasicProperties.Headers;
                var correlationId = headers!.GetCorrelationId();
                var schemaVersion = headers!.GetSchemaVersion();
                var traceParent = headers!.GetTraceParent();

                var messageId = ea.BasicProperties.MessageId;
                //set correlationId from header for end to end correlation
                CorrelationContext.CorrelationId = correlationId;

                activity?.AddTag("rabbitmq.queue", queueName);
                activity?.AddTag("rabbitmq.deadletter.exchange", $"{exchangeName}.dlq");
                activity?.AddTag("rabbitmq.exchange", exchangeName);
                activity?.SetTag("message.id", messageId);
                activity?.SetTag("schema.version", schemaVersion);
                activity?.SetTag("correlation.id", correlationId);
                activity?.AddTag("trace-parent", traceParent);

                if (schemaVersion != expectedSchemaVersion)
                    throw new SchemaValidationException($"Schema mismatch. schemaVersion:{schemaVersion}  expectedSchemaVersion:{expectedSchemaVersion}");

                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                _logger.LogInformation("📤 Rabbitmq consumer - message json {json}- IntegrationEvent {eventType} - EventId {EventId}", json, ea.BasicProperties.Type!, ea.BasicProperties.MessageId);

                bool IsValid = await ValidateSchema(ea.BasicProperties.Type!, schemaVersion, json);
                activity?.SetTag("schema.valid", IsValid);

                var message = JsonSerializer.Deserialize<TEvent>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;

                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<TEvent>>();

                await handler.HandleAsync(message, ct);

                await channel.BasicAckAsync(ea.DeliveryTag, false);

                _logger.LogInformation("📤 Rabbitmq consumer - message processed succesfully - IntegrationEvent {eventType}", ea.BasicProperties.Type!);

                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (SchemaValidationException ex)
            {
                await SendToDlq(channel, ea, ex.Message);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
                activity?.SetTag("send.to.dlq", true);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(ex, "❌ Rabbitmq consumer - Error in message processing -> SendToDlq . {EventType} | Queue={Queue} | MessageId={MessageId}", typeof(TEvent).Name, queueName, ea.BasicProperties.MessageId);

            }
            catch (DbUpdateException ex)
            {
                await Retry(channel, exchangeName, ea);
                activity?.SetTag("send.to.retry", true);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(ex, "❌ Rabbitmq consumer - Error in message processing -> SendToRetry . {EventType} | Queue={Queue} | MessageId={MessageId}", typeof(TEvent).Name, queueName, ea.BasicProperties.MessageId);

            }
            catch (Exception ex)
            {
                await SendToDlq(channel, ea, ex.Message);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
                activity?.SetTag("send.to.dlq", true);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(ex, "❌ Rabbitmq consumer - Error in message processing -> SendToDlq . {EventType} | Queue={Queue} | MessageId={MessageId}", typeof(TEvent).Name, queueName, ea.BasicProperties.MessageId);

            }
        }

        private async Task Retry(IChannel channel, string exchangeName, BasicDeliverEventArgs ea)
        {
            var headers = ea.BasicProperties.Headers;
            var retryCount = headers!.GetRetryCount();
            _logger.LogInformation("📤 Rabbitmq consumer - sending to retry - IntegrationEvent {eventType} - RetryCount {retryCount}", ea.BasicProperties.Type!, retryCount);

            if (retryCount >= _maxRetryCount)
            {
                _logger.LogInformation("📤 Rabbitmq consumer - sending to retry - IntegrationEvent {eventType} - RetryCount {retryCount}", ea.BasicProperties.Type!, retryCount);

                await SendToDlq(channel, ea, "MaxRetryCount reached.");
                return;
            }

            var nextRoutingKey = retryCount switch
            {
                0 => "retry.1m",
                1 => "retry.5m",
                _ => "dlq"
            };

            ea.BasicProperties.Headers![RabbitHeaders.RetryCount] = retryCount + 1;
            var properties = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = ea.BasicProperties.MessageId,
                Type = ea.BasicProperties.Type,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Headers = ea.BasicProperties.Headers != null
                    ? new Dictionary<string, object?>(ea.BasicProperties.Headers)
                    : new Dictionary<string, object?>()
            };

            await channel.BasicPublishAsync(
                exchange: $"{exchangeName}.retry",
                routingKey: nextRoutingKey,
                mandatory: true,
                basicProperties: properties,
                body: ea.Body);

            await channel.BasicAckAsync(ea.DeliveryTag, false);

            _logger.LogInformation(
               "☠️ Message sent to retry | Exchange={Exchange} | EventType={EventType} | RetryCount={retryCount} | MessageId={MessageId}",
               ea.Exchange,
               ea.BasicProperties.Type,
               retryCount,
               ea.BasicProperties.MessageId);
        }


        private void ValidateHeaders(IDictionary<string, object?>? headers)
        {
            if (headers == null)
                throw new HeaderValidationException("Headers are missing.");

            var requiredHeaders = new[]
            {
               RabbitHeaders.RetryCount,
               RabbitHeaders.CorrelationId,
               RabbitHeaders.SchemaVersion
           };

            foreach (var header in requiredHeaders)
            {
                if (!headers.ContainsKey(header))
                    throw new HeaderValidationException($"Required header '{header}' is missing.");
            }

            // Validate types
            if (!(headers[RabbitHeaders.RetryCount] is byte[] msgIdBytes) || msgIdBytes.Length == 0)
                throw new HeaderValidationException($"Header '{RabbitHeaders.RetryCount}' is empty or invalid.");

            if (!(headers[RabbitHeaders.CorrelationId] is byte[] corrIdBytes) || corrIdBytes.Length == 0)
                throw new HeaderValidationException($"Header '{RabbitHeaders.CorrelationId}' is empty or invalid.");

            if (!(headers[RabbitHeaders.CausationId] is byte[] causationBytes) || causationBytes.Length == 0)
                throw new HeaderValidationException($"Header '{RabbitHeaders.CausationId}' is empty or invalid.");

            if (!(headers[RabbitHeaders.SchemaVersion] is byte[] schemaBytes) || schemaBytes.Length == 0)
                throw new HeaderValidationException($"Header '{RabbitHeaders.SchemaVersion}' is empty or invalid.");
        }


        private async Task SendToDlq(IChannel channel, BasicDeliverEventArgs ea, string reason, string sourceQueue = "")
        {
            _logger.LogInformation("📤 Rabbitmq consumer - sending to dlq - IntegrationEvent {eventType}", ea.BasicProperties.Type!);

            // Preserve original metadata
            var properties = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = ea.BasicProperties.MessageId,
                Type = ea.BasicProperties.Type,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Headers = ea.BasicProperties.Headers != null
                ? new Dictionary<string, object?>(ea.BasicProperties.Headers)
                : new Dictionary<string, object?>()
            };

            // DLQ forensic headers
            properties.Headers[DlqHeaders.Reason] = Encoding.UTF8.GetBytes(reason);
            properties.Headers[DlqHeaders.ExceptionType] = Encoding.UTF8.GetBytes(reason.GetType().Name);
            properties.Headers[DlqHeaders.FailedAtUtc] = Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O"));
            properties.Headers[DlqHeaders.SourceQueue] = Encoding.UTF8.GetBytes(sourceQueue);

            // Ensure retry count is preserved
            if (!properties.Headers.ContainsKey(RabbitHeaders.RetryCount))
                properties.Headers[RabbitHeaders.RetryCount] = 0;

            await channel.BasicPublishAsync(
                exchange: "techassistpro.dlq",
                routingKey: string.Empty, // fanout
                mandatory: true,
                basicProperties: properties,
                body: ea.Body);

            _logger.LogInformation(
                "☠️ Message sent to DLQ | Exchange={Exchange} | EventType={EventType} | Reason={Reason} | MessageId={MessageId}",
                ea.Exchange,
                ea.BasicProperties.Type,
                reason,
                ea.BasicProperties.MessageId);
        }


        private async Task<bool> ValidateSchema(
            string eventType,
            int schemaVersion,
            string json)
        {

            var isValid = await _schemaRegistry.ValidateAsync(eventType, schemaVersion, json);

            if (!isValid)
            {
                var errorMsg = $"Rabbitmq consumer - Event {eventType} v{schemaVersion} failed schema validation";
                throw new SchemaValidationException(errorMsg);

            }
            ;
            return isValid;
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