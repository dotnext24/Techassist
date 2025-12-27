using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using TechAssistPro.Infrastructure.Observability;
using TechAssistPro.Ticketing.Data;
using TechAssistPro.Ticketing.Dtos;
using TechAssistPro.Ticketing.Events;

namespace TechAssistPro.Ticketing.Application.Commands
{
    public sealed class CreateTicketCommandHandler
    : IRequestHandler<CreateTicketCommand, TicketResponseDto>
    {
        private readonly ITicketRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateTicketCommandHandler> _logger;
        private readonly ActivitySource _activitySource;
        public CreateTicketCommandHandler(ITicketRepository repository, IMapper mapper, ILogger<CreateTicketCommandHandler> logger, ActivitySource activitySource)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _activitySource = activitySource;

        }

        public async Task<TicketResponseDto> Handle(
            CreateTicketCommand request,
            CancellationToken cancellationToken)
        {

            using var activity = _activitySource.StartActivity("Create-Ticket");
            activity?.SetTag("customer.id", request.CustomerId);

            _logger.LogInformation("Create-Ticket started | CustomerId={CustomerId}", request.CustomerId);
            try
            {
                var ticket = Ticket.Create(
                    request.CustomerId,
                    request.Subject,
                    request.Description,
                    request.Category,
                    request.Priority,
                    request.Channel,
                    request.CreatedBy);

                await _repository.AddAsync(ticket, cancellationToken);

                activity?.SetTag("ticket.id", ticket.Id);
                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation("Create-Ticket succeeded | CustomerId={CustomerId}", request.CustomerId);


                return _mapper.Map<TicketResponseDto>(ticket);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(
                    ex,
                    "Error in Create-Ticket command handler {CustomerId}",
                    request.CustomerId);

                throw;
            }
        }
    }
}