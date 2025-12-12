// Ticketing/Infrastructure/Data/TicketDbContext.cs
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class TicketDbContext : DbContextBase
{
    public TicketDbContext(
        DbContextOptions<TicketDbContext> options,
        IMediator mediator)
        : base(options, mediator) { }

    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(builder =>
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.CustomerId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Subject)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Description)
                .IsRequired(false);

            builder.Property(t => t.Category)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(t => t.Priority)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(t => t.Channel)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(t => t.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(t => t.AssignedTechnicianId)
                .IsRequired(false);

            builder.Property(t => t.CreatedAtUtc)
                .IsRequired();

            builder.Property(t => t.LastUpdatedAtUtc)
                .IsRequired();

            builder.Property(t => t.UpdatedBy)
            .IsRequired(false);

            builder.Property(t => t.LastUpdatedAtUtc)
                .IsRequired(false);

            builder.HasQueryFilter(x => !x.IsDeleted);
        });

    }

}

