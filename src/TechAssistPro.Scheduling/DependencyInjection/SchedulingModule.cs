using TechAssistPro.Infrastructure.Messaging;
using TechAssistPro.Infrastructure.SchemaRegistry;
using TechAssistPro.Scheduling.Application.Commands;
using TechAssistPro.Scheduling.Events;
using TechAssistPro.Scheduling.HostedServices;
using TechAssistPro.Scheduling.Services;
using TechAssistPro.SharedKernel.Events;
using TechAssistPro.Scheduling.Data;
using TechAssistPro.Infrastructure.Events;
using Microsoft.AspNetCore.Rewrite;
using TechAssistPro.Scheduling.Middleware;
using Microsoft.EntityFrameworkCore;
using TechAssistPro.SharedKernel.Responses;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TechAssistPro.Scheduling.DependencyInjection
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
            RegisterServices(services);

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

            services.AddDbContext<SchedulingDbContext>(options =>
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
            services.AddScoped<IResponseFactory, ResponseFactory>();
            services.AddScoped<ISupportAgentRepository, SupportAgentRepository>();
            services.AddScoped<IAssignmentRepository, AssignmentRepository>();
            services.AddSingleton<ISchemaRegistry, SchemaRegistry>();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<ISupportAgentMatcher, SkillBasedSupportAgentMatcher>();

        }

        private static void RegisterEventHandlers(IServiceCollection services)
        {
            services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
            services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
            services.AddScoped<IEventHandler<SupportAgentAssignedDomainEvent>, AssignmentCreatedEventHandler>();

            services.AddScoped<IIntegrationEventHandler<TicketCreatedIntegrationEvent>, TicketCreatedHandler>();
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

                cfg.RegisterServicesFromAssemblyContaining<AutoAssignSupportAgentCommandHandler>();
                cfg.RegisterServicesFromAssemblyContaining<AssignmentCreatedEventHandler>();

            });
        }

        private static void ConfigureAutoMapper(IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
                {
                    // custom config here (optional)
                    cfg.AllowNullCollections = true;
                }, typeof(AutoAssignSupportAgentCommandHandler).Assembly);
        }

        private static void ConfigureFluentValidation(IServiceCollection services)
        {
            //services.AddValidatorsFromAssemblyContaining<SupportAgentValidator>();
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
                    Title = "TechAssistPro Scheduling API",
                    Version = "v1",
                    Description = "API for managing support agents and assignments"
                });
            });
        }

        #endregion
    }


}