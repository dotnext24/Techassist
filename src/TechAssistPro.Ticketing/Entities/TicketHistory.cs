using TechAssistPro.Ticketing.Enums;
using TechAssistPro.SharedKernel.Domain;

namespace TechAssistPro.Ticketing.Entities
{
    public class TicketHistory : AggregateRoot
    {
        public Guid UserId { get; private set; } = default!;
        public string Comment { get; private set; } = default!;
        public DateTimeOffset Timestamp { get; private set; } = default!;
        public TicketStatus StatusAtTime { get; private set; } = default!;

        // EF Core constructor
        private TicketHistory() { }

        public TicketHistory(Guid id, Guid userId, string comment, DateTimeOffset timestamp, TicketStatus statusAtTime)
        {
            Id = id;
            UserId = userId;
            Comment = comment;
            Timestamp = timestamp;
            StatusAtTime = statusAtTime;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public static TicketHistory Create(
           Guid userId, string comment, DateTimeOffset timestamp, TicketStatus statusAtTime,
           string createdBy)
        {
            Guid id = Guid.NewGuid();
            var ticketHistory = new TicketHistory(
            id,
            userId,
            comment,
            timestamp,
            statusAtTime);
            ticketHistory.Touch(createdBy);
            return ticketHistory;

        }

        private void Touch(string updatedBy)
        {
            LastUpdatedAtUtc = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

    }
}