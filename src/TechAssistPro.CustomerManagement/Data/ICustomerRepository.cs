using System;
using System.Threading;
using TechAssistPro.CustomerManagement.Entities;
using TechAssistPro.SharedKernel.Pagination;

namespace TechAssistPro.CustomerManagement.Data
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<PagedResult<Customer>> GetPagedAsync(int page, int size, Guid? customerId, CancellationToken ct);
        Task AddAsync(Customer customer, CancellationToken ct);
        Task UpdateAsync(Customer customer, CancellationToken ct);
        Task SoftDeleteAsync(Customer customer, CancellationToken ct);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    }
}