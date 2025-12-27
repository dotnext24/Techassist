using TechAssistPro.Scheduling.Entities;
using TechAssistPro.Scheduling.Enums;
using TechAssistPro.SharedKernel.Pagination;

namespace TechAssistPro.Scheduling.Data
{
    public interface IAssignmentRepository
    {
        Task<Assignment?> GetByTicketIdAsync(Guid ticketId,CancellationToken ct);
        Task<Assignment?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PagedResult<Assignment>> GetPagedAsync(
            int page, int size,
            Guid? supportAgentId,
            AssignmentStatus? status,
            CancellationToken ct);

        Task AddAsync(Assignment agent, CancellationToken ct = default);
        Task UpdateAsync(Assignment agent, CancellationToken ct = default);
        Task SoftDeleteAsync(Assignment agent, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    }
}