using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TechAssistPro.Infrastructure.Observability;
using TechAssistPro.Scheduling.Entities;
using TechAssistPro.Scheduling.Enums;
using TechAssistPro.SharedKernel.Pagination;

namespace TechAssistPro.Scheduling.Data
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly SchedulingDbContext _db;
        private readonly ActivitySource _activitySource;
        private readonly ILogger<AssignmentRepository> _logger;
        public AssignmentRepository(SchedulingDbContext db, ActivitySource activitySource, ILogger<AssignmentRepository> logger)
        {
            _db = db;
            _activitySource = activitySource;
            _logger = logger;
        }

        public async Task<Assignment?> GetByTicketIdAsync(
        Guid ticketId,
        CancellationToken ct)
        {
            using var activity = _activitySource.StartActivity("Get-Assignment-GetByTicketId");
            activity?.SetTag("db.operation", "SELECT");
            activity?.SetTag("entity", "Assignment");
            activity?.SetTag("ticket.Id", ticketId);
            activity?.SetTag("correlation.id", CorrelationContext.CorrelationId);

            _logger.LogInformation("Get-Assignment-ByTicketId started");

            var stopwatch = Stopwatch.StartNew();

            try{
            var assignment= await _db.Assignments
                .FirstOrDefaultAsync(a => a.TicketId == ticketId, ct);
            
             stopwatch.Stop();
                
                activity?.SetTag("db.duration_ms", stopwatch.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation("Get-Assignment-ByTicketId finished | {Duration}ms", stopwatch.ElapsedMilliseconds);

                return assignment;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(
                    ex,
                    "Database error in fetching assignment by ticketId | TicketId={TicketId}",
                    ticketId);

                throw;
            }
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
            using var activity = _activitySource.StartActivity("AddTicket");
            activity?.SetTag("db.operation", "INSERT");
            activity?.SetTag("entity", "Assignment");
            activity?.SetTag("ticket.id", assignment.TicketId);
            activity?.SetTag("correlation.id", CorrelationContext.CorrelationId);

            _logger.LogInformation("AddAssignment started | TicketId={TicketId}", assignment.TicketId);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _db.Assignments.AddAsync(assignment, ct);
                await _db.SaveChangesAsync(ct);
                stopwatch.Stop();
                activity?.SetTag("assignment.id", assignment.Id);
                activity?.SetTag("db.duration_ms", stopwatch.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation("Assignment persisted to database | AssignmentId={AssignmentId} | {Duration}ms", assignment.Id, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(
                    ex,
                    "Database error persisting assignment | TicketId={TicketId}",
                    assignment.TicketId);

                throw;
            }
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
