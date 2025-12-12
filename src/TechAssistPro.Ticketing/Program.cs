using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechAssistPro.SharedKernel.Responses;
using TechAssistPro.Ticketing.API;
using TechAssistPro.Ticketing.Application.Commands;
using TechAssistPro.Ticketing.Application.Validation;
using TechAssistPro.Ticketing.Data;
using TechAssistPro.Ticketing.Mapping;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------
// PostgreSQL compatibility switch
// -----------------------------------------
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(o => o.SuppressAsyncSuffixInActionNames = false);

// -----------------------------------------
// DbContext
// -----------------------------------------
builder.Services.AddDbContext<TicketDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TechAssistDb"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "ticketing");
        }));



// --------------------------------------------------
// 2. Register Repositories
// --------------------------------------------------
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IResponseFactory, ResponseFactory>();
// --------------------------------------------------
// 3. MediatR
// --------------------------------------------------
// MediatR (correct for MediatR 12+)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<CreateTicketCommand>();
});

// --------------------------------------------------
// 4. AutoMapper
// --------------------------------------------------
builder.Services.AddAutoMapper(cfg =>
{
    // custom config here (optional)
    cfg.AllowNullCollections = true;

}, typeof(TicketMappingProfile).Assembly);

// --------------------------------------------------
// 5. FluentValidation
// --------------------------------------------------
builder.Services.AddValidatorsFromAssemblyContaining<TicketValidator>();

// --------------------------------------------------
// 6. JSON Options
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// --------------------------------------------------
// 10. Minimal API Endpoints (Ticket Endpoints)
// --------------------------------------------------
TicketEndpoints.Map(app);
try{
app.Run();
}
catch(Exception ex)
{
    Console.Write(ex.ToString());
}
