public record CreateTicketDto(
    string CustomerId,
    string Subject,
    string Description,
    string Category,
    string Priority,
    string Channel,
    string CreatedBy
);
