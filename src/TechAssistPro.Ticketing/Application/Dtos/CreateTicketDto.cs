public record CreateTicketDto(
    string CustomerId,
    string Subject,
    string Description,
    TicketCategory Category,
    TicketPriority Priority,
    TicketChannel Channel,
    string CreatedBy
);
