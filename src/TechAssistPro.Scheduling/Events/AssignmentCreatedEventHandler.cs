using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TechAssistPro.Infrastructure.Observability;
using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.Scheduling.Events
{
    public sealed class AssignmentCreatedEventHandler : IEventHandler<SupportAgentAssignedDomainEvent>
    {

        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<AssignmentCreatedEventHandler> _logger;
        private readonly ActivitySource _activitySource;
        public AssignmentCreatedEventHandler(IEventPublisher eventPublisher, ILogger<AssignmentCreatedEventHandler> logger, ActivitySource activitySource)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
            _activitySource=activitySource;
        }
        public async Task Handle(SupportAgentAssignedDomainEvent domainEvent, CancellationToken ct)
        {
            using var activity = _activitySource.StartActivity("Assignment-Created-Event");
         
            activity?.SetTag("ticket.id", domainEvent.Data.TicketId);
            activity?.SetTag("event-type", domainEvent.EventType);
            activity?.SetTag("correlation.id", CorrelationContext.CorrelationId);
            
            _logger.LogInformation("Assignment-Created-Event started | TicketId={TicketId}", domainEvent.Data.TicketId);

            try
            {
                await _eventPublisher.PublishAsync(domainEvent.EventType, domainEvent, domainEvent.SchemaVersion, ct);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(ex, "Assignment-Created-Event failed | TicketId={TicketId} | {EventType}", domainEvent.Data.TicketId,domainEvent.EventType);
            }
        }
    }
}