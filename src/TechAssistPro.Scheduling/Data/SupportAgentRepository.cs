using Microsoft.EntityFrameworkCore;
using TechAssistPro.Scheduling.Entities;
using TechAssistPro.SharedKernel.Pagination;

namespace TechAssistPro.Scheduling.Data
{
    public class SupportAgentRepository : ISupportAgentRepository
    {
        private readonly SchedulingDbContext _db;

        public SupportAgentRepository(SchedulingDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<SupportAgent>> GetAvailableAsync(
        CancellationToken ct)
        {
            return await _db.SupportAgents
                .Where(a => true) //Where(a => a.Availability.IsAvailable)
                .ToListAsync(ct);
        }

        public async Task<SupportAgent?> GetByIdAsync(Guid id, CancellationToken ct)
            => await _db.SupportAgents.FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<PagedResult<SupportAgent>> GetPagedAsync(
            int page, int size,
            string? search,
            CancellationToken ct)
        {
            var query = _db.SupportAgents.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.Name.Contains(search));

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return new PagedResult<SupportAgent>(page, size, total, items);
        }

        public async Task AddAsync(SupportAgent agent, CancellationToken ct)
        {
            await _db.SupportAgents.AddAsync(agent, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(SupportAgent agent, CancellationToken ct)
        {
            _db.SupportAgents.Update(agent);
            await _db.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(SupportAgent agent, CancellationToken ct)
        {
            _db.SupportAgents.Update(agent);
            await _db.SaveChangesAsync(ct);
        }


        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.SupportAgents.AnyAsync(x => x.Id == id);
        }


    }


}
