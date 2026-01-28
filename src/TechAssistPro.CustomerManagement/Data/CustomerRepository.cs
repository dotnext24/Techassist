// src/TechAssistPro.CustomerManagement/Data/CustomerRepository.cs
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TechAssistPro.Infrastructure.Observability;
using TechAssistPro.CustomerManagement.Entities;
using TechAssistPro.SharedKernel.Pagination;

namespace TechAssistPro.CustomerManagement.Data
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly CustomerManagementDbContext _db;
        private readonly ActivitySource _activitySource;
        private readonly ILogger<CustomerRepository> _logger;
        public CustomerRepository(CustomerManagementDbContext db, ActivitySource activitySource, ILogger<CustomerRepository> logger)
        {
            _db = db;
            _activitySource = activitySource;
            _logger = logger;
        }
        public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct)
            => await _db.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<PagedResult<Customer>> GetPagedAsync(
            int page, int size,
            Guid? customerId,
            CancellationToken ct)
        {
            var query = _db.Customers.AsQueryable();

            if (customerId.HasValue)
                query = query.Where(x => x.Id == customerId.Value);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return new PagedResult<Customer>(page, size, total, items);
        }

        public async Task AddAsync(Customer customer, CancellationToken ct)
        {
            using var activity = _activitySource.StartActivity("AddCustomer");
            activity?.SetTag("db.operation", "INSERT");
            activity?.SetTag("entity", "Customer");
            activity?.SetTag("customer.id", customer.Id);
            activity?.SetTag("correlation.id", CorrelationContext.CorrelationId);

            _logger.LogInformation("AddCustomer started | CustomerId={CustomerId}", customer.Id);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _db.Customers.AddAsync(customer, ct);
                await _db.SaveChangesAsync(ct);
                stopwatch.Stop();
                activity?.SetTag("customer.id", customer.Id);
                activity?.SetTag("db.duration_ms", stopwatch.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation("Customer persisted to database | CustomerId={CustomerId} | {Duration}ms", customer.Id, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(
                    ex,
                    "Database error persisting customer | CustomerId={CustomerId}",
                    customer.Id);

                throw;
            }
        }

        public async Task UpdateAsync(Customer customer, CancellationToken ct)
        {
            _db.Customers.Update(customer);
            await _db.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Customer customer, CancellationToken ct)
        {
            _db.Customers.Update(customer);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Customers.AnyAsync(x => x.Id == id);
        }
    }
}