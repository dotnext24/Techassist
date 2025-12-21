using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using TechAssistPro.Infrastructure.Messaging;
using TechAssistPro.Infrastructure.SchemaRegistry;
using TechAssistPro.Scheduling.DependencyInjection;
using TechAssistPro.Scheduling.Middleware;

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
builder.Services.AddDbContext<SchedulingDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TechAssistDb"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "scheduling");
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

// -----------------------------------------
// 4. Application Services
// -----------------------------------------
builder.Services.AddServices(builder.Configuration);



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

//Redirect to api doc
var option = new RewriteOptions();
option.AddRedirect("^$", "swagger");
app.UseRewriter(option);

app.UseAuthorization();

// --------------------------------------------------
// 10. Custom Exception Handling Middleware
// --------------------------------------------------
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();
