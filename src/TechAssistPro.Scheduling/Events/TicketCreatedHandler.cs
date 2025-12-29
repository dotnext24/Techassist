using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using TechAssistPro.Scheduling.Application.Commands;
using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.Scheduling.Events;

public sealed class TicketCreatedHandler
    : IIntegrationEventHandler<TicketCreatedIntegrationEvent>
{
    private readonly ILogger<TicketCreatedHandler> _logger;
    private readonly IMediator _mediator;
    private readonly ActivitySource _activitySource;
    public TicketCreatedHandler(IMediator mediator, ILogger<TicketCreatedHandler> logger, ActivitySource activitySource)
    {
        _mediator = mediator;
        _logger = logger;
        _activitySource = activitySource;
    }

    public async Task HandleAsync(
        TicketCreatedIntegrationEvent @event,
        CancellationToken ct)
    {

        using var activity = _activitySource.StartActivity("Ticket-Created-Event");
        activity?.SetTag("ticket.id", @event.Data.TicketId);
        activity?.SetTag("ticket.category", @event.Data.Category);
        activity?.SetTag("event-type", @event.EventType);

        _logger.LogInformation("🎯Ticket-Created-Event started | TicketId={TicketId}", @event.Data.TicketId);
        try
        {
            await _mediator.Send(
                new AutoAssignSupportAgentCommand(@event.Data.TicketId, @event.Data.Category, @event.Data.Priority),
                ct);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogInformation("Ticket-Created-Event failed | TicketId={TicketId}", @event.Data.TicketId);
            // optionally persist to outbox
        }

    }
}


