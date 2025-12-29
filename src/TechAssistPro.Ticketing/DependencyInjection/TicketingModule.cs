using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using TechAssistPro.Infrastructure.Events;
using TechAssistPro.Infrastructure.Messaging;
using TechAssistPro.Infrastructure.SchemaRegistry;
using TechAssistPro.SharedKernel.Events;
using TechAssistPro.SharedKernel.Responses;
using TechAssistPro.Ticketing.Application.Commands;
using TechAssistPro.Ticketing.Application.Validation;
using TechAssistPro.Ticketing.Data;
using TechAssistPro.Ticketing.Events;
using TechAssistPro.Ticketing.HostedServices;
using TechAssistPro.Ticketing.Mapping;
using TechAssistPro.Ticketing.Middleware;


namespace TechAssistPro.Ticketing.DependencyInjection
{
    public static class TicketingModule
    {
      public static IServiceCollection AddInfrastructure(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var config = builder.Configuration;

        ConfigurePostgreSQL(services, config);
        ConfigureRabbitMQ(services, config);
        RegisterRepositories(services);
        RegisterEventHandlers(services);
        RegisterHostedServices(services);

        return services;
    }

    public static IServiceCollection AddApplication(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        ConfigureMediatR(services);
        ConfigureAutoMapper(services);
        ConfigureFluentValidation(services);

        return services;
    }

    public static IServiceCollection AddApi(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        ConfigureMvcOptions(services);
        ConfigureJsonOptions(services);
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        ConfigureSwagger(services);

        return services;
    }

    public static async Task InitializeSchemaRegistryAsync(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Initializing Schema Registry");

            var schemaRegistry = app.Services.GetRequiredService<ISchemaRegistry>();

            await schemaRegistry.RegisterSchemaFromFileAsync(
                "ticket.created", 1, "Schemas/ticket-created-v1.json");

            await schemaRegistry.RegisterSchemaFromFileAsync(
                "support.agent.assigned", 1, "Schemas/support-agent-assigned-v1.json");

            logger.LogInformation("Schema Registry initialized successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize Schema Registry");
            throw;
        }
    }

    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseRewriter(new RewriteOptions().AddRedirect("^$", "swagger"));
        app.UseAuthorization();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseRequestLogging();

        return app;
    }

    #region Private Configuration Methods

    private static void ConfigurePostgreSQL(IServiceCollection services, IConfiguration config)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddDbContext<TicketDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("TechAssistDb"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "ticketing");
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                }));
    }

    private static void ConfigureRabbitMQ(IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IRabbitMQConnection>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMQConnection>>();
            var uri = config.GetConnectionString("RabbitMQ") 
                ?? throw new InvalidOperationException("RabbitMQ connection string is missing");
            
            return new RabbitMQConnection(uri, logger);
        });

        services.Configure<MessagingOptions>(config.GetSection("Messaging"));
        services.AddSingleton<RabbitMqEventSubscriber>();
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IResponseFactory, ResponseFactory>();
        services.AddSingleton<ISchemaRegistry, SchemaRegistry>();
    }

    private static void RegisterEventHandlers(IServiceCollection services)
    {
        services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
        services.AddScoped<IEventHandler<TicketCreatedDomainEvent>, TicketCreatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<SupportAgentAssignedIntegrationEvent>,
            SupportAgentAssignedHandler>();
    }

    private static void RegisterHostedServices(IServiceCollection services)
    {
        services.AddHostedService<EventSubscriptionHostedService>();
    }

    private static void ConfigureMediatR(IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DomainEventNotificationHandler).Assembly);
            cfg.RegisterServicesFromAssemblyContaining<CreateTicketCommandHandler>();
            cfg.RegisterServicesFromAssemblyContaining<SupportAgentAssignedHandler>();
        });
    }

    private static void ConfigureAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AllowNullCollections = true;
        }, typeof(TicketMappingProfile).Assembly);
    }

    private static void ConfigureFluentValidation(IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<TicketValidator>();
    }

    private static void ConfigureMvcOptions(IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        });
    }

    private static void ConfigureJsonOptions(IServiceCollection services)
    {
        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.WriteIndented = true;
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
    }

    private static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "TechAssistPro Ticketing API",
                Version = "v1",
                Description = "API for managing support tickets"
            });
        });
    }

    #endregion
}


}