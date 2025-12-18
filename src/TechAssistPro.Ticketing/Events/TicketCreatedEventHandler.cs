using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.Ticketing.Events
{
    public class TicketCreatedEventHandler : IEventHandler<TicketCreatedDomainEvent>
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<TicketCreatedEventHandler> _logger;
        public TicketCreatedEventHandler(IEventPublisher eventPublisher, ILogger<TicketCreatedEventHandler> logger)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task Handle(
            TicketCreatedDomainEvent domainEvent,
            CancellationToken ct)
        {
            _logger.LogInformation("TicketCreatedEventHandler Called");
            try
            {
                await _eventPublisher.PublishAsync(
                    domainEvent.EventType,
                    domainEvent.Data,
                    domainEvent.SchemaVersion,
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish domain event {EventType}", domainEvent.EventType);
                // optionally persist to outbox
            }
        }
    }
}