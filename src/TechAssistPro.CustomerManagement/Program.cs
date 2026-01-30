using Serilog;
using TechAssistPro.CustomerManagement.API;
using TechAssistPro.CustomerManagement.DependencyInjection;


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
CustomerEndpoints.Map(app);


// Run Application
try
{
    Log.Information("Starting TechAssistPro.CustomerManagement service");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "TechAssistPro.CustomerManagement service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}



