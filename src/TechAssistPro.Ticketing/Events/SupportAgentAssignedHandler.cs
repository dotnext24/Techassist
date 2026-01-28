
using MediatR;
using TechAssistPro.Ticketing.Application.Commands;
using TechAssistPro.SharedKernel.Events;
using TechAssistPro.Infrastructure.Observability;
using System.Diagnostics;

namespace TechAssistPro.Ticketing.Events;

public sealed class SupportAgentAssignedHandler
    : IIntegrationEventHandler<SupportAgentAssignedIntegrationEvent>
{
    private readonly ActivitySource _activitySource;
    private readonly ILogger<SupportAgentAssignedHandler> _logger;
    private readonly IMediator _mediator;
    public SupportAgentAssignedHandler(ActivitySource activitySource, IMediator mediator, ILogger<SupportAgentAssignedHandler> logger)
    {
        _activitySource = activitySource;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task HandleAsync(
        SupportAgentAssignedIntegrationEvent @event,
        CancellationToken ct)
    {
        using var activity = _activitySource.StartActivity("Support-Agent-Assigned-Event");
        activity?.SetTag("ticket.id", @event.Data.TicketId);
        activity?.SetTag("support-agent-id", @event.Data.SupportAgentId);
        activity?.SetTag("event-type", @event.EventType);
        activity?.AddTag("correlation.id", CorrelationContext.CorrelationId);

        _logger.LogInformation("🎯 Support-Agent-Assigned-Event started | TicketId={TicketId}", @event.Data.TicketId);

        try
        {
            await _mediator.Send(
                new AssignSupportAgentCommand(@event.Data.TicketId, @event.Data.AssignmentId, @event.Data.SupportAgentId),
                ct);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogInformation("Support-Agent-Assigned-Event failed | TicketId={TicketId}", @event.Data.TicketId);
            // optionally persist to outbox
        }

    }
}


