using RabbitMQ.Client;
using System.Threading.Tasks;

namespace TechAssistPro.Infrastructure.Messaging
{
   public interface IRabbitMQConnection
    {
        Task<IConnection> ConnectAsync();
        Task<IChannel> CreateChannelAsync();
    }
}