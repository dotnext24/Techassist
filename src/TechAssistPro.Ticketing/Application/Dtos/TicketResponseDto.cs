using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechAssistPro.Ticketing.Dtos
{
    public record TicketResponseDto(
    Guid Id,
    string CustomerId,
    string Subject,
    string Description,
    TicketCategory Category,
    TicketPriority Priority,
    TicketChannel Channel,
    TicketStatus Status,
    string? AssignedTechnicianId,
    DateTime CreatedAtUtc,
    string? UpdatedBy,
    DateTime? LastUpdatedAtUtc
);


}