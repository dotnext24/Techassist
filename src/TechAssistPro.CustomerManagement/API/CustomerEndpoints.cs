using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using TechAssistPro.Infrastructure.Events;
using TechAssistPro.SharedKernel.Responses;
using TechAssistPro.CustomerManagement.Application;


namespace TechAssistPro.CustomerManagement.API
{
    public static class CustomerEndpoints
    {
        public static void Map(WebApplication app)
        {
            var group = app.MapGroup("/api/customers");

            group.MapPost("/", async ValueTask<IResult> (CustomerCreateDto request, IResponseFactory responder, IMapper mapper, IMediator mediator, IValidator<CustomerCreateDto> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(responder.Error(errors: validationResult.ToDictionary()));

                }
                var command = mapper.Map<CustomerCreateCommand>(request);
                var result = await mediator.Send(command);
                return Results.Ok(responder.Success(result));
            });

            // group.MapGet("/", async ([AsParameters] GetCustomersQuery query, IMediator mediator) =>
            //     await mediator.Send(query));

            // group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            //     await mediator.Send(new GetCustomerByIdQuery(id)));
        
        }
    }
}