using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechAssistPro.Infrastructure.Messaging;
using TechAssistPro.Infrastructure.SchemaRegistry;
using TechAssistPro.Scheduling.Application.Commands;
using TechAssistPro.Scheduling.Events;
using TechAssistPro.Scheduling.HostedServices;
using TechAssistPro.Scheduling.Services;
using TechAssistPro.SharedKernel.Events;
using TechAssistPro.Ticketing.Data;

namespace TechAssistPro.Scheduling.DependencyInjection
{
    public static class SchedulingModule
    {
        public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
        {

        // -----------------------------
        // Repositories
        // -----------------------------
        services.AddScoped<ISupportAgentRepository, SupportAgentRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();

        // -----------------------------
        // Services
        // -----------------------------
        services.AddScoped<ISupportAgentMatcher, SkillBasedSupportAgentMatcher>();
        services.AddSingleton<ISchemaRegistry, SchemaRegistry>();


        // -----------------------------
        // Application (MediatR)
        // -----------------------------
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(AutoAssignSupportAgentCommandHandler).Assembly));


        // -----------------------------
        // Integration Event Handlers
        // -----------------------------
        services.AddSingleton<
            IIntegrationEventHandler<TicketCreatedIntegrationEvent>,
            TicketCreatedHandler>();
        


        // -----------------------------
        // RabbitMQ Subscriber
        // -----------------------------
        services.AddSingleton<RabbitMqEventSubscriber>();

        // -----------------------------
        // Hosted Services
        // -----------------------------
        services.AddHostedService<EventSubscriptionHostedService>();





            return services;
        }
    }
}