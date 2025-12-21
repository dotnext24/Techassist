using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using TechAssistPro.Infrastructure.Messaging;
using TechAssistPro.Infrastructure.SchemaRegistry;
using TechAssistPro.Infrastructure.Events;
using TechAssistPro.SharedKernel.Events;
using TechAssistPro.SharedKernel.Responses;
using TechAssistPro.Ticketing.API;
using TechAssistPro.Ticketing.Application.Commands;
using TechAssistPro.Ticketing.Application.Validation;
using TechAssistPro.Ticketing.Data;
using TechAssistPro.Ticketing.Mapping;
using TechAssistPro.Ticketing.Events;
using TechAssistPro.Ticketing.Middleware;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------
// 1.PostgreSQL compatibility switch
// -----------------------------------------
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);


builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(o => o.SuppressAsyncSuffixInActionNames = false);

// -----------------------------------------
// 2.DbContext
// -----------------------------------------
builder.Services.AddDbContext<TicketDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TechAssistDb"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "ticketing");
        }));

// -----------------------------------------
// 3.RabbitMQ Connection Setting
// -----------------------------------------
builder.Services.AddSingleton<IRabbitMQConnection>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RabbitMQConnection>>();
    var uri = builder.Configuration.GetConnectionString("RabbitMQ")!;

    return new RabbitMQConnection(uri, logger);
});

// --------------------------------------------------
// 4. Register Repositories
// --------------------------------------------------
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IResponseFactory, ResponseFactory>();
builder.Services.AddSingleton<ISchemaRegistry, SchemaRegistry>();

builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
builder.Services.AddScoped<IEventHandler<TicketCreatedDomainEvent>,TicketCreatedEventHandler>();


// --------------------------------------------------
// 5. MediatR
// --------------------------------------------------
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(DomainEventNotificationHandler).Assembly);

    cfg.RegisterServicesFromAssemblyContaining<CreateTicketCommandHandler>();
    
});


// --------------------------------------------------
// 6. AutoMapper
// --------------------------------------------------
builder.Services.AddAutoMapper(cfg =>
{
    // custom config here (optional)
    cfg.AllowNullCollections = true;

}, typeof(TicketMappingProfile).Assembly);

// --------------------------------------------------
// 7. FluentValidation
// --------------------------------------------------
builder.Services.AddValidatorsFromAssemblyContaining<TicketValidator>();

// --------------------------------------------------
// 8. JSON Options
// --------------------------------------------------
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --------------------------------------------------
// 9. SchemaRegistry for event message contract validation on start
// --------------------------------------------------
var schemaRegistry = app.Services.GetRequiredService<ISchemaRegistry>();
await schemaRegistry.RegisterSchemaFromFileAsync(
    "ticket.created",
    1,
    "Schemas/ticket-created-v1.json");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// --------------------------------------------------
// 10. Custom Exception Handling Middleware
// --------------------------------------------------
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

// --------------------------------------------------
// 11. Minimal API Endpoints (Ticket Endpoints)
// --------------------------------------------------
TicketEndpoints.Map(app);

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.Write(ex.ToString());
}
