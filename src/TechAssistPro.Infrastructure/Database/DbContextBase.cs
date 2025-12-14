// Infrastructure/Database/DbContextBase.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechAssistPro.Infrastructure.Events;

public abstract class DbContextBase : DbContext
{
    private readonly IMediator _mediator;

    protected DbContextBase(DbContextOptions options, IMediator mediator)
        : base(options) => _mediator = mediator;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before commit
        
        var result = await base.SaveChangesAsync(cancellationToken);

        await this.DispatchDomainEventsAsync(_mediator, cancellationToken);

        return result;
    }
}
