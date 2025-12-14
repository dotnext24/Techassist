
namespace TechAssistPro.SharedKernel.Events
{
    public interface IEventPublisher
    {
        Task PublishAsync(
            string eventType,
            object eventData,
            int schemaVersion,
            CancellationToken cancellationToken = default);
    }
}