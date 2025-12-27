using Microsoft.EntityFrameworkCore;
using TechAssistPro.Scheduling.Entities;
using TechAssistPro.Scheduling.Enums;
using TechAssistPro.SharedKernel.Pagination;

namespace TechAssistPro.Scheduling.Data
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly SchedulingDbContext _db;

        public AssignmentRepository(SchedulingDbContext db)
        {
            _db = db;
        }

        public async Task<Assignment?> GetByTicketIdAsync(
        Guid ticketId,
        CancellationToken ct)
        {
            return await _db.Assignments
                .FirstOrDefaultAsync(a => a.TicketId == ticketId, ct);
        }

        public async Task<Assignment?> GetByIdAsync(Guid id, CancellationToken ct)
            => await _db.Assignments.FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<PagedResult<Assignment>> GetPagedAsync(
            int page, int size,
            Guid? supportAgentId,
            AssignmentStatus? status,
            CancellationToken ct)
        {
            var query = _db.Assignments.AsQueryable();

            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);

            if (status.HasValue)
                query = query.Where(x => x.SupportAgentId == supportAgentId.Value);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return new PagedResult<Assignment>(page, size, total, items);
        }

        public async Task AddAsync(Assignment assignment, CancellationToken ct)
        {
            await _db.Assignments.AddAsync(assignment, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Assignment assignment, CancellationToken ct)
        {
            _db.Assignments.Update(assignment);
            await _db.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Assignment assignment, CancellationToken ct)
        {
            _db.Assignments.Update(assignment);
            await _db.SaveChangesAsync(ct);
        }


        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Assignments.AnyAsync(x => x.Id == id);
        }


    }


}
