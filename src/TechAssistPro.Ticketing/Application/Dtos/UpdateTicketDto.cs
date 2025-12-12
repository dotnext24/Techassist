using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechAssistPro.Ticketing.Dtos
{
    public record UpdateTicketDto(
     string Subject,
     string Description,
     TicketCategory Category,
     TicketPriority Priority,
     string UpdatedBy
 );

}