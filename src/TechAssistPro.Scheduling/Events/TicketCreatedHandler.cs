using System;
using System.Collections.Generic;
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
    public TicketCreatedHandler(IMediator mediator,ILogger<TicketCreatedHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task HandleAsync(
        TicketCreatedIntegrationEvent @event,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "🎯 TicketCreated IntegrationEvent handled | TicketId: {Id}",
            @event.Data.TicketId);

        await _mediator.Send(
            new AutoAssignSupportAgentCommand(@event.Data.TicketId,@event.Data.Category,@event.Data.Priority),
            ct);
        
    }
}


