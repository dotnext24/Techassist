using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechAssistPro.SharedKernel.Pagination;

namespace TechAssistPro.Ticketing.Data
{
    public interface ITicketRepository
    {
        Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PagedResult<Ticket>> GetPagedAsync(
        int page, int size,
        string? search,
        TicketCategory? category,
        TicketStatus? status,
        CancellationToken ct);

        Task AddAsync(Ticket ticket, CancellationToken ct = default);
        Task UpdateAsync(Ticket ticket, CancellationToken ct = default);
        Task SoftDeleteAsync(Ticket ticket, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    }
}