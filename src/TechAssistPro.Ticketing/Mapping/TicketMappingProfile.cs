using AutoMapper;
using TechAssistPro.Ticketing.Application.Commands;
using TechAssistPro.Ticketing.Dtos;
using TechAssistPro.SharedKernel.Common;

namespace TechAssistPro.Ticketing.Mapping
{
    public class TicketMappingProfile : Profile
    {
        public TicketMappingProfile()
        {
            CreateMap<Ticket, TicketResponseDto>()
            .ConstructUsing(t => new TicketResponseDto(
         t.Id,
         t.CustomerId,
         t.Subject,
         t.Description,
         t.Category,
         t.Priority,
         t.Channel,
         t.Status,
         t.AssignedTechnicianId,
         t.CreatedAtUtc,
         t.UpdatedBy,
         t.LastUpdatedAtUtc
     ));


            CreateMap<CreateTicketDto, CreateTicketCommand>()
            .ForCtorParam(nameof(CreateTicketCommand.Category),
             opt => opt.MapFrom(src => EnumParser.Parse<TicketCategory>(src.Category)))
            .ForCtorParam(nameof(CreateTicketCommand.Priority),
             opt => opt.MapFrom(src => EnumParser.Parse<TicketPriority>(src.Priority)))
            .ForCtorParam(nameof(CreateTicketCommand.Channel),
             opt => opt.MapFrom(src => EnumParser.Parse<TicketChannel>(src.Channel)));


            CreateMap<UpdateTicketDto, Ticket>()
                .ForAllMembers(opts => opts.Ignore()); // updates handled in domain
        }
    }
}