using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechAssistPro.SharedKernel.Pagination;

namespace TechAssistPro.Ticketing.Data
{
    public class TicketRepository : ITicketRepository
    {
        private readonly TicketDbContext _db;

        public TicketRepository(TicketDbContext db)
        {
            _db = db;
        }

        public async Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct)
            => await _db.Tickets.FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<PagedResult<Ticket>> GetPagedAsync(
            int page, int size,
            string? search,
            TicketCategory? category,
            TicketStatus? status,
            CancellationToken ct)
        {
            var query = _db.Tickets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.Subject.Contains(search));

            if (category.HasValue)
                query = query.Where(x => x.Category == category.Value);

            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return new PagedResult<Ticket>(page, size, total, items);
        }

        public async Task AddAsync(Ticket ticket, CancellationToken ct)
        {
            await _db.Tickets.AddAsync(ticket, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Ticket ticket, CancellationToken ct)
        {
            _db.Tickets.Update(ticket);
            await _db.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Ticket ticket, CancellationToken ct)
        {
            _db.Tickets.Update(ticket);
            await _db.SaveChangesAsync(ct);
        }


        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Tickets.AnyAsync(x => x.Id == id);
        }


    }


}
