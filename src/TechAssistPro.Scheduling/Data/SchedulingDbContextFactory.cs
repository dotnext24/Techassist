using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MediatR;

namespace TechAssistPro.Scheduling.Data;

public class SchedulingDbContextFactory : IDesignTimeDbContextFactory<SchedulingDbContext>
{
    public SchedulingDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                          ?? "Development";
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .Build();

        // Create DbContextOptionsBuilder
        var optionsBuilder = new DbContextOptionsBuilder<SchedulingDbContext>();

        // Configure your database provider (example with SQL Server)
        optionsBuilder.UseNpgsql(
            configuration.GetConnectionString("TechAssistDb"));

        // Create a dummy IMediator for design-time (it won't be used during migrations)
        IMediator mediator = null!; // or use a mock/stub if needed

        return new SchedulingDbContext(optionsBuilder.Options, mediator);
    }
}