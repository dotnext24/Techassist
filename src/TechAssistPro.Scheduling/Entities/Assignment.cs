using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechAssistPro.Scheduling.Enums;
using TechAssistPro.Scheduling.Events;
using TechAssistPro.SharedKernel.Domain;
using TechAssistPro.SharedKernel.Exceptions;

namespace TechAssistPro.Scheduling.Entities
{
    public sealed class Assignment : AggregateRoot
    {
        public Guid TicketId { get; private set; }
        public Guid SupportAgentId { get; private set; }
        public AssignmentStatus Status { get; private set; }

        public int ReassignmentCount { get; private set; }

        private Assignment() { }

        private Assignment(
            Guid id,
            Guid ticketId,
            Guid supportAgentId)
            : base(id)
        {
            TicketId = ticketId;
            SupportAgentId = supportAgentId;
            Status = AssignmentStatus.Assigned;
            CreatedAtUtc = DateTime.UtcNow;
            ReassignmentCount = 0;

        }

        public static Assignment Create(
            Guid ticketId,
            Guid supportAgentId,
            string? createdBy)
        {
            var assignment = new Assignment(
                Guid.NewGuid(),
                ticketId,
                supportAgentId);

            assignment.Touch(createdBy);
            // Raise domain events if any
            assignment.AddCreatedEvent();

            return assignment;
        }

        public void Start(string? updatedBy)
        {
            if (Status != AssignmentStatus.Assigned)
                throw new InvalidOperationException();

            Status = AssignmentStatus.InProgress;
            Touch(updatedBy);
        }

        public void Complete(string? updatedBy)
        {
            if (Status != AssignmentStatus.InProgress)
                throw new InvalidOperationException();

            Status = AssignmentStatus.Completed;
            Touch(updatedBy);
        }

        public void Reassign(Guid newAgentId, string? updatedBy)
        {
            if (Status == AssignmentStatus.Completed)
                throw new InvalidOperationException();

            if (newAgentId == SupportAgentId)
                throw new DomainException("Cannot reassign to same agent");

            SupportAgentId = newAgentId;
            Status = AssignmentStatus.Reassigned;
            ReassignmentCount++;
            Touch(updatedBy);
        }

        public void SoftDelete(string? deletedBy)
        {
            IsDeleted = true;
            Touch(deletedBy);
        }

        private void Touch(string? updatedBy)
        {
            LastUpdatedAtUtc = DateTime.UtcNow;
            UpdatedBy = updatedBy ?? "System";
        }

        private void AddCreatedEvent()
        {
            RaiseDomainEvent(
                new SupportAgentAssignedDomainEvent(this));
        }
    }

}