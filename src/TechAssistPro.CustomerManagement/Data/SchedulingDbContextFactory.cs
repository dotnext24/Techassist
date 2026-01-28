using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MediatR;

namespace TechAssistPro.CustomerManagement.Data;

public class CustomerManagementDbContextFactory : IDesignTimeDbContextFactory<CustomerManagementDbContext>
{
    public CustomerManagementDbContext CreateDbContext(string[] args)
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
        var optionsBuilder = new DbContextOptionsBuilder<CustomerManagementDbContext>();

        // Configure your database provider (example with SQL Server)
        optionsBuilder.UseNpgsql(
            configuration.GetConnectionString("TechAssistDb"));

        // Create a dummy IMediator for design-time (it won't be used during migrations)
        IMediator mediator = null!; // or use a mock/stub if needed

        return new CustomerManagementDbContext(optionsBuilder.Options, mediator);
    }
}