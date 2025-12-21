using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
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
        public CreateTicketCommandHandler(ITicketRepository repository, IMapper mapper, ILogger<CreateTicketCommandHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;

        }

        public async Task<TicketResponseDto> Handle(
            CreateTicketCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("CreateTicketCommandHandler Called");
            var ticket = Ticket.Create(
                request.CustomerId,
                request.Subject,
                request.Description,
                request.Category,
                request.Priority,
                request.Channel,
                request.CreatedBy);              

            await _repository.AddAsync(ticket, cancellationToken);
            
            return _mapper.Map<TicketResponseDto>(ticket);
        }
    }
}