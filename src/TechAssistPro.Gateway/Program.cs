using System.Text;
using Serilog;
using TechAssistPro.Gateway.Middleware;
using TechAssistPro.Gateway.HealthChecks;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
// Add Serilog
builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


// Add Rate Limiting
//builder.Services.AddCustomRateLimiting();

// Add Health Checks
builder.Services.AddHealthChecks();

// Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
{
    c.RoutePrefix = ""; // Swagger at root

    c.SwaggerEndpoint(
        "/ticketing/swagger/v1/swagger.json",
        "Ticketing API");

    c.SwaggerEndpoint(
        "/scheduling/swagger/v1/swagger.json",
        "Scheduling API");
    c.SwaggerEndpoint(
        "/customer/swagger/v1/swagger.json",
        "Customer API");
});
}

// Custom middleware
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<CircuitBreakerMiddleware>();


app.UseCors();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Rate Limiting
//app.UseRateLimiter


app.MapControllers();

// Map health checks
app.MapHealthChecks("/health");

// Map reverse proxy
app.MapReverseProxy();

Log.Information("API Gateway starting...");
app.Run();

