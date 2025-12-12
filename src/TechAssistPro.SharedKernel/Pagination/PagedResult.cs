
namespace TechAssistPro.SharedKernel.Pagination
{
    public class PagedResult<T>
    {
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public IEnumerable<T> Items { get; } = Enumerable.Empty<T>();

        public PagedResult(int page, int pageSize, int totalCount, IEnumerable<T> items)
        {
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
            Items = items;
        }
    }
}