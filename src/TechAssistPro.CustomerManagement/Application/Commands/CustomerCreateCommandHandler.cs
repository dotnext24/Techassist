using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using TechAssistPro.Infrastructure.Observability;
using TechAssistPro.CustomerManagement.Data;
using TechAssistPro.CustomerManagement.Application;
using TechAssistPro.CustomerManagement.Entities;

namespace TechAssistPro.CustomerManagement.Application.Commands
{
    public sealed class CustomerCreateCommandHandler
    : IRequestHandler<CustomerCreateCommand, CustomerCreateResponse>
    {
        private readonly ICustomerRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerCreateCommandHandler> _logger;
        private readonly ActivitySource _activitySource;
        public CustomerCreateCommandHandler(ICustomerRepository repository, IMapper mapper, ILogger<CustomerCreateCommandHandler> logger, ActivitySource activitySource)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _activitySource = activitySource;
        }

        public async Task<CustomerCreateResponse> Handle(
            CustomerCreateCommand request,
            CancellationToken cancellationToken)
        {

            using var activity = _activitySource.StartActivity("Create-Customer");
            activity?.SetTag("customer.name", request.Name);
            activity?.SetTag("correlation.id", CorrelationContext.CorrelationId);
            
            _logger.LogInformation("Create-Customer started | CustomerName={CustomerName}", request.Name);
            
            try
            {
                var customer = Customer.Create(
                    request.Name,
                    request.Email,
                    request.PhoneNumber,
                    request.Address,
                    request.CreatedBy);

                await _repository.AddAsync(customer, cancellationToken);

                activity?.SetTag("customer.id", customer.Id);
                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation("Create-Customer succeeded | CustomerName={CustomerName}", request.Name);


                return _mapper.Map<CustomerCreateResponse>(customer);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(
                    ex,
                    "Error in Create-Customer command handler {CustomerName}",
                    request.Name);

                throw;
            }
        }
    }
}