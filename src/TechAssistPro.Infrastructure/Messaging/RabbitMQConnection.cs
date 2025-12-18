using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace TechAssistPro.Infrastructure.Messaging
{
    public class RabbitMQConnection : IRabbitMQConnection, IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private readonly ILogger<RabbitMQConnection> _logger;
        public RabbitMQConnection(string amqpUri, ILogger<RabbitMQConnection> logger)
        {
            _logger = logger;
            _logger.LogInformation("amqpUri:"+amqpUri);
            _factory = new ConnectionFactory
            {
                Uri = new Uri(amqpUri),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };
            
        }

        public async Task<IConnection> ConnectAsync()
        {
            if (_connection is { IsOpen: true })
                return _connection;

            _connection = await _factory.CreateConnectionAsync();
            return _connection;
        }

        public async Task<IChannel> CreateChannelAsync()
        {
            var connection = await ConnectAsync();
            return await connection.CreateChannelAsync();
        }


        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_connection != null)
                {
                    _connection?.CloseAsync();
                    _connection?.DisposeAsync();
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}