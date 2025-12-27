// Ticketing/Infrastructure/Data/TicketDbContext.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechAssistPro.Scheduling.Entities;

namespace TechAssistPro.Scheduling.Data;
public sealed class SchedulingDbContext : DbContextBase
{
    public SchedulingDbContext(
        DbContextOptions<SchedulingDbContext> options,
        IMediator mediator)
        : base(options, mediator) { }

    public DbSet<SupportAgent> SupportAgents => Set<SupportAgent>();
    public DbSet<Assignment> Assignments => Set<Assignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SupportAgent>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.Property(a => a.Id)
               .ValueGeneratedNever();

            builder.Property(a => a.Name)
               .IsRequired()
               .HasMaxLength(200);

            builder.Property(a => a.ActiveAssignments)
               .IsRequired();

            builder.OwnsOne(a => a.Availability, b =>
            {
                b.Property(v => v.IsAvailable)
                 .HasColumnName("IsAvailable")
                 .IsRequired();
            });

            builder.OwnsMany(a => a.Skills, b =>
            {
                b.ToTable("SupportAgentSkills");

                b.WithOwner()
                 .HasForeignKey("SupportAgentId");

                b.Property<Guid>("Id");
                b.HasKey("Id");

                b.Property(s => s.Category)
                 .IsRequired()
                 .HasMaxLength(100);

                b.Property(s => s.Level)
                 .IsRequired();
            });

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


        modelBuilder.Entity<Assignment>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.Property(a => a.Id)
               .ValueGeneratedNever();

            builder.Property(a => a.TicketId)
               .IsRequired();

            builder.Property(a => a.SupportAgentId)
               .IsRequired();

            builder.Property(a => a.Status)
               .IsRequired()
               .HasConversion<string>();

            builder.HasIndex(a => a.TicketId)
               .IsUnique();

            builder.Property(a => a.ReassignmentCount)
               .IsRequired();

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

        // Seed SupportAgents
        SeedSupportAgents(modelBuilder);
    }


    private static void SeedSupportAgents(ModelBuilder modelBuilder)
    {
        // Fixed IDs for migrations (VERY IMPORTANT)
        var agent1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var agent2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var agent3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var agent4Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var agent5Id = Guid.Parse("55555555-5555-5555-5555-555555555555");

        modelBuilder.Entity<SupportAgent>().HasData(
            new
            {
                Id = agent1Id,
                Name = "Arjun",
                ActiveAssignments = 0,
                CreatedAtUtc = DateTime.UtcNow,
                IsDeleted = false
            },
            new
            {
                Id = agent2Id,
                Name = "Priya",
                ActiveAssignments = 0,
                CreatedAtUtc = DateTime.UtcNow,
                IsDeleted = false
            },
            new
            {
                Id = agent3Id,
                Name = "Rohit",
                ActiveAssignments = 0,
                CreatedAtUtc = DateTime.UtcNow,
                IsDeleted = false
            },
            new
            {
                Id = agent4Id,
                Name = "Sneha",
                ActiveAssignments = 0,
                CreatedAtUtc = DateTime.UtcNow,
                IsDeleted = false
            },
            new
            {
                Id = agent5Id,
                Name = "Karthik",
                ActiveAssignments = 0,
                CreatedAtUtc = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        // Owned type: Availability
        modelBuilder.Entity<SupportAgent>()
            .OwnsOne(a => a.Availability)
            .HasData(
                new { SupportAgentId = agent1Id, IsAvailable = true },
                new { SupportAgentId = agent2Id, IsAvailable = true },
                new { SupportAgentId = agent3Id, IsAvailable = true },
                new { SupportAgentId = agent4Id, IsAvailable = true },
                new { SupportAgentId = agent5Id, IsAvailable = true }
            );

        // Owned collection: Skills
        modelBuilder.Entity<SupportAgent>()
            .OwnsMany(a => a.Skills)
            .HasData(
                // Agent 1
                new { Id = Guid.NewGuid(), SupportAgentId = agent1Id, Category = "Hardware", Level = 3 },
                new { Id = Guid.NewGuid(), SupportAgentId = agent1Id, Category = "Network", Level = 2 },

                // Agent 2
                new { Id = Guid.NewGuid(), SupportAgentId = agent2Id, Category = "Software", Level = 3 },

                // Agent 3
                new { Id = Guid.NewGuid(), SupportAgentId = agent3Id, Category = "Database", Level = 2 },
                new { Id = Guid.NewGuid(), SupportAgentId = agent3Id, Category = "Software", Level = 2 },

                // Agent 4
                new { Id = Guid.NewGuid(), SupportAgentId = agent4Id, Category = "Network", Level = 3 },

                // Agent 5
                new { Id = Guid.NewGuid(), SupportAgentId = agent5Id, Category = "Hardware", Level = 2 }
            );
    }

}

