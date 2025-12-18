
using RabbitMQ.Client;
using System.Text;
using TechAssistPro.SharedKernel.Events;
using TechAssistPro.Infrastructure.SchemaRegistry;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.DependencyInjection;

namespace TechAssistPro.Infrastructure.Messaging
{
    public class RabbitMqEventSubscriber : IDisposable
    {
        private readonly IRabbitMQConnection _connection;
        private readonly ISchemaRegistry _schemaRegistry;

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMqEventSubscriber> _logger;

        private readonly List<IChannel> _channels = new();
        public RabbitMqEventSubscriber(IRabbitMQConnection connection, ISchemaRegistry schemaRegistry, IServiceProvider serviceProvider, ILogger<RabbitMqEventSubscriber> logger)
        {
            _connection = connection;
            _schemaRegistry = schemaRegistry;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task SubscribeAsync<TEvent>(
          string queueName,
          string[] routingKeys,
          CancellationToken ct = default)
          where TEvent : class
        {
            var channel = await _connection.CreateChannelAsync();
            _channels.Add(channel);

            // Exchanges
            await channel.ExchangeDeclareAsync(
                "ticket.events",
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
                    { "x-dead-letter-exchange", "ticket.events.dq" }
                });

            foreach (var routingKey in routingKeys)
            {
                await channel.QueueBindAsync(
                    queue: queueName,
                    exchange: "ticket.events",
                    routingKey: routingKey);
            }

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                    var message = JsonSerializer.Deserialize<TEvent>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        })!;

                    using var scope = _serviceProvider.CreateScope();

                    var handler = scope.ServiceProvider
                        .GetRequiredService<IIntegrationEventHandler<TEvent>>();

                    await handler.HandleAsync(message, ct);

                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "❌ Error processing {EventType}", typeof(TEvent).Name);

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
                "🐇 Subscribed to {EventType} on {Queue}",
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