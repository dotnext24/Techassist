using Serilog;
using TechAssistPro.Ticketing.API;
using TechAssistPro.Ticketing.DependencyInjection;


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



var app = builder.Build();

// Initialize Schema Registry
await app.InitializeSchemaRegistryAsync();


// Configure Middleware Pipeline
app.ConfigureMiddleware();

// Map Endpoints
app.MapControllers();
TicketEndpoints.Map(app);


// Run Application
try
{
    Log.Information("Starting TechAssistPro.Ticketing service");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "TechAssistPro.Ticketing service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}



