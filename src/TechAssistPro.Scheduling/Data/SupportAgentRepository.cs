using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TechAssistPro.Infrastructure.Observability;
using TechAssistPro.Scheduling.Entities;
using TechAssistPro.SharedKernel.Pagination;

namespace TechAssistPro.Scheduling.Data
{
    public class SupportAgentRepository : ISupportAgentRepository
    {
        private readonly SchedulingDbContext _db;
        private readonly ActivitySource _activitySource;
        private readonly ILogger<SupportAgentRepository> _logger;

        public SupportAgentRepository(SchedulingDbContext db, ActivitySource activitySource, ILogger<SupportAgentRepository> logger)
        {
            _db = db;
            _activitySource = activitySource;
            _logger = logger;
        }

        public async Task<IEnumerable<SupportAgent>> GetAvailableAsync(
        CancellationToken ct)
        {
            using var activity = _activitySource.StartActivity("Get-Available-SupportAgent");
            activity?.SetTag("db.operation", "SELECT");
            activity?.SetTag("entity", "Assignment");
            activity?.SetTag("correlation.id", CorrelationContext.CorrelationId);

            _logger.LogInformation("Get-Available-SupportAgent started");

            var stopwatch = Stopwatch.StartNew();

            try
            {

                var agents = await _db.SupportAgents
                    .Where(a => true) //Where(a => a.Availability.IsAvailable)
                    .ToListAsync(ct);

                stopwatch.Stop();

                activity?.SetTag("db.duration_ms", stopwatch.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation("Get-Available-SupportAgent finished | {Duration}ms", stopwatch.ElapsedMilliseconds);


                return agents;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(
                    ex,
                    "Database error in fetching support Agents");

                throw;
            }
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
