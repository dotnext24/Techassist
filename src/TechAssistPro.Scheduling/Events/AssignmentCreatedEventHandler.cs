using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.Scheduling.Events
{
    public sealed class AssignmentCreatedEventHandler : IEventHandler<SupportAgentAssignedDomainEvent>
    {

        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<AssignmentCreatedEventHandler> _logger;
        public AssignmentCreatedEventHandler(IEventPublisher eventPublisher, ILogger<AssignmentCreatedEventHandler> logger)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
        }
        public async Task Handle(SupportAgentAssignedDomainEvent domainEvent, CancellationToken ct)
        {
            _logger.LogInformation("AssignmentCreatedEventHandler Called");
            try
            {
                await _eventPublisher.PublishAsync(domainEvent.EventType, domainEvent, domainEvent.SchemaVersion, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish domain event {EventType}", domainEvent.EventType);
            }
        }
    }
}