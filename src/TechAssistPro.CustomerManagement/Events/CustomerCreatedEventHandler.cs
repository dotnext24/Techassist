using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TechAssistPro.Infrastructure.Observability;
using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.CustomerManagement.Events
{
    public sealed class CustomerCreatedEventHandler : IEventHandler<CustomerCreatedDomainEvent>
    {

        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<CustomerCreatedEventHandler> _logger;
        private readonly ActivitySource _activitySource;
        public CustomerCreatedEventHandler(IEventPublisher eventPublisher, ILogger<CustomerCreatedEventHandler> logger, ActivitySource activitySource)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
            _activitySource=activitySource;
        }
        public async Task Handle(CustomerCreatedDomainEvent domainEvent, CancellationToken ct)
        {
            using var activity = _activitySource.StartActivity("Customer-Created-Event");
           
            activity?.SetTag("customer.id", domainEvent.Data.CustomerId);
            activity?.SetTag("event-type", domainEvent.EventType);
            activity?.SetTag("correlation.id", CorrelationContext.CorrelationId);
            
            _logger.LogInformation("Customer-Created-Event started | CustomerId={CustomerId}", domainEvent.Data.CustomerId);

            try
            {
                await _eventPublisher.PublishAsync(domainEvent.EventType, domainEvent, domainEvent.SchemaVersion, ct);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(ex, "Customer-Created-Event failed | CustomerId={CustomerId} | {EventType}", domainEvent.Data.CustomerId,domainEvent.EventType);
            }
        }
    }
}