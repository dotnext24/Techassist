using MediatR;
using Microsoft.EntityFrameworkCore;
using TechAssistPro.CustomerManagement.Entities;

namespace TechAssistPro.CustomerManagement.Data;
public sealed class CustomerManagementDbContext : DbContextBase
{
    public CustomerManagementDbContext(
        DbContextOptions<CustomerManagementDbContext> options,
        IMediator mediator)
        : base(options, mediator) { }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.Property(a => a.Id)
               .ValueGeneratedNever();

            builder.Property(a => a.Name)
               .IsRequired()
               .HasMaxLength(200);

            builder.Property(a => a.Email)
               .IsRequired()
               .HasMaxLength(200);

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