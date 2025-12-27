using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TechAssistPro.SharedKernel.Pagination;

namespace TechAssistPro.Ticketing.Data
{
    public class TicketRepository : ITicketRepository
    {
        private readonly TicketDbContext _db;
        private readonly ActivitySource _activitySource;
        private readonly ILogger<TicketRepository> _logger;
        public TicketRepository(TicketDbContext db, ActivitySource activitySource, ILogger<TicketRepository> logger)
        {
            _db = db;
            _activitySource = activitySource;
            _logger = logger;
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

            using var activity = _activitySource.StartActivity("AddTicket");
            activity?.SetTag("db.operation", "INSERT");
            activity?.SetTag("entity", "Ticket");
            activity?.SetTag("customer.id", ticket.CustomerId);

            _logger.LogInformation("AddTicket started | CustomerId={CustomerId}", ticket.CustomerId);
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _db.Tickets.AddAsync(ticket, ct);
                await _db.SaveChangesAsync(ct);

                stopwatch.Stop();
                activity?.SetTag("ticket.id", ticket.Id);
                activity?.SetTag("db.duration_ms", stopwatch.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation("Ticket persisted to database | TicketId={TicketId} | {Duration}ms", ticket.Id, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(
                    ex,
                    "Database error persisting ticket {CustomerId}",
                    ticket.CustomerId);

                throw;
            }

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
