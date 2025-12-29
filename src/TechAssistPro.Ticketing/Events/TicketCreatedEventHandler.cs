using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TechAssistPro.Infrastructure.Observability;
using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.Ticketing.Events
{
    public class TicketCreatedEventHandler : IEventHandler<TicketCreatedDomainEvent>
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<TicketCreatedEventHandler> _logger;
        private readonly ActivitySource _activitySource;
        public TicketCreatedEventHandler(IEventPublisher eventPublisher, ILogger<TicketCreatedEventHandler> logger, ActivitySource activitySource)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
            _activitySource=activitySource;
        }

        public async Task Handle(
            TicketCreatedDomainEvent domainEvent,
            CancellationToken ct)
        {
            
            using var activity = _activitySource.StartActivity("Create-Ticket-Event");
            activity?.SetTag("ticket.id", domainEvent.Data.TicketId);
            activity?.SetTag("event-type", domainEvent.EventType);
            activity?.SetTag("correlation.id", CorrelationContext.CorrelationId);

            _logger.LogInformation("Create-Ticket-Event started | TicketId={TicketId}", domainEvent.Data.TicketId);
            
            try
            {
                await _eventPublisher.PublishAsync(
                    domainEvent.EventType,
                    domainEvent,
                    domainEvent.SchemaVersion,
                    ct);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(ex, "Create-Ticket-Event failed | TicketId={TicketId} | {EventType}", domainEvent.Data.TicketId,domainEvent.EventType);
                
            }
        }
    }
}