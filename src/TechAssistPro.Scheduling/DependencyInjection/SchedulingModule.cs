using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TechAssistPro.Infrastructure.Messaging;
using TechAssistPro.Infrastructure.SchemaRegistry;
using TechAssistPro.Scheduling.Application.Commands;
using TechAssistPro.Scheduling.Events;
using TechAssistPro.Scheduling.HostedServices;
using TechAssistPro.Scheduling.Services;
using TechAssistPro.SharedKernel.Events;
using TechAssistPro.Scheduling.Data;
using TechAssistPro.Infrastructure.Events;

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
            services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
            services.AddScoped<IEventHandler<SupportAgentAssignedDomainEvent>, AssignmentCreatedEventHandler>();


            // Application (MediatR)
            // -----------------------------
            // -----------------------------         

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(
                    typeof(DomainEventNotificationHandler).Assembly);

                cfg.RegisterServicesFromAssemblyContaining<AutoAssignSupportAgentCommandHandler>();
                cfg.RegisterServicesFromAssemblyContaining<AssignmentCreatedEventHandler>();

            });


            // -----------------------------
            // Mapper
            // -----------------------------        
            services.AddAutoMapper(cfg =>
            {
                // custom config here (optional)
                cfg.AllowNullCollections = true;
            }, typeof(AutoAssignSupportAgentCommandHandler).Assembly);

            // -----------------------------
            // Integration Event Handlers
            // -----------------------------
            services.AddScoped<
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