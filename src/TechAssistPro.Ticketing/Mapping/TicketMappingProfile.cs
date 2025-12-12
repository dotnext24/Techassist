using AutoMapper;
using TechAssistPro.Ticketing.Dtos;

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


            CreateMap<CreateTicketDto, Ticket>()
                .ConstructUsing(src =>
                    new Ticket(
                        src.CustomerId,
                        src.Subject,
                        src.Description,
                        src.Category,
                        src.Priority,
                        src.Channel));

            CreateMap<UpdateTicketDto, Ticket>()
                .ForAllMembers(opts => opts.Ignore()); // updates handled in domain
        }
    }
}