using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using TechAssistPro.Ticketing.Data;
using TechAssistPro.Ticketing.Dtos;

namespace TechAssistPro.Ticketing.Application.Commands
{
    public sealed class CreateTicketCommandHandler
    : IRequestHandler<CreateTicketCommand, TicketResponseDto>
    {
        private readonly ITicketRepository _repository;
        private readonly IMapper _mapper;
        public CreateTicketCommandHandler(ITicketRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;

        }

        public async Task<TicketResponseDto> Handle(
            CreateTicketCommand request,
            CancellationToken cancellationToken)
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

            return _mapper.Map<TicketResponseDto>(ticket);
        }
    }
}