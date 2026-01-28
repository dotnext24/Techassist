using TechAssistPro.Infrastructure.Messaging;
using TechAssistPro.Infrastructure.SchemaRegistry;
using TechAssistPro.CustomerManagement.Application.Commands;
using TechAssistPro.CustomerManagement.Application.Validations;
using TechAssistPro.CustomerManagement.Events;
using TechAssistPro.SharedKernel.Events;
using TechAssistPro.CustomerManagement.Data;
using TechAssistPro.Infrastructure.Events;
using Microsoft.AspNetCore.Rewrite;
using TechAssistPro.CustomerManagement.Middleware;
using Microsoft.EntityFrameworkCore;
using TechAssistPro.SharedKernel.Responses;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;


namespace TechAssistPro.CustomerManagement.DependencyInjection
{
    public static class CustomerManagementModule
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
                    "customer.created", 1, "Schemas/customer-created-v1.json");

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

            services.AddDbContext<CustomerManagementDbContext>(options =>
                options.UseNpgsql(
                    config.GetConnectionString("TechAssistDb"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "customer_management");
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
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddSingleton<ISchemaRegistry, SchemaRegistry>();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            // Add services here
        }

        private static void RegisterEventHandlers(IServiceCollection services)
        {
            services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
            services.AddScoped<IEventHandler<CustomerCreatedDomainEvent>, CustomerCreatedEventHandler>();
        }

        private static void RegisterHostedServices(IServiceCollection services)
        {
            // Add hosted services here
        }

        private static void ConfigureMediatR(IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(DomainEventNotificationHandler).Assembly);

            cfg.RegisterServicesFromAssemblyContaining<CustomerCreateCommandHandler>();
                // Add MediatR configurations here
            });
        }

        private static void ConfigureAutoMapper(IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
                {
                    // custom config here (optional)
                    cfg.AllowNullCollections = true;
                }, typeof(CustomerManagementModule).Assembly);
        }

        private static void ConfigureFluentValidation(IServiceCollection services)
        {
             services.AddValidatorsFromAssemblyContaining<CustomerCreateValidator>();
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
                    Title = "TechAssistPro Customer Management API",
                    Version = "v1",
                    Description = "API for managing customers"
                });
            });
        }

        #endregion
    }
}