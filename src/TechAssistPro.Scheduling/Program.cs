using Serilog;
using TechAssistPro.Scheduling.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Observability: Logging & Tracing
builder.AddLogger();
builder.AddTracing();


// Infrastructure: Database, Messaging, Repositories, Services
builder.AddInfrastructure();


// Application: MediatR, Validation, Mapping
builder.AddApplication();


// API: Controllers, Swagger, JSON
builder.AddApi();

builder.Services.AddHealthChecks();


var app = builder.Build();

// Map health checks
app.MapHealthChecks("/health");

// Initialize Schema Registry
await app.InitializeSchemaRegistryAsync();


// Configure Middleware Pipeline
app.ConfigureMiddleware();

// Map Endpoints
app.MapControllers();


// Run Application
try
{
    Log.Information("Starting TechAssistPro.Scheduling service");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "TechAssistPro.Scheduling service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

