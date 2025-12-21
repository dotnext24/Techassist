using TechAssistPro.Scheduling.Entities;
using TechAssistPro.SharedKernel.Pagination;

namespace TechAssistPro.Ticketing.Data
{
    public interface ISupportAgentRepository
    {
        Task<IEnumerable<SupportAgent>> GetAvailableAsync(CancellationToken ct = default);
        Task<SupportAgent?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PagedResult<SupportAgent>> GetPagedAsync(
        int page, int size,
        string? search,
        CancellationToken ct);

        Task AddAsync(SupportAgent agent, CancellationToken ct = default);
        Task UpdateAsync(SupportAgent agent, CancellationToken ct = default);
        Task SoftDeleteAsync(SupportAgent agent, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    }
}