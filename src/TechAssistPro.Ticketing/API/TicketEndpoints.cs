using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using TechAssistPro.Infrastructure.Events;
using TechAssistPro.SharedKernel.Responses;
using TechAssistPro.Ticketing.Application.Commands;

namespace TechAssistPro.Ticketing.API
{
    public static class TicketEndpoints
    {
        public static void Map(WebApplication app)
        {
            var group = app.MapGroup("/api/tickets");

            group.MapPost("/", async ValueTask<IResult> (CreateTicketDto request, IResponseFactory responder, IMapper mapper, IMediator mediator, IValidator<CreateTicketDto> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(responder.Error(errors: validationResult.ToDictionary()));

                }
                var command = mapper.Map<CreateTicketCommand>(request);
                var result = await mediator.Send(command);
                return Results.Ok(responder.Success(result));
            });

//             group.MapGet("/ping", async (IMediator mediator) =>
// {
//     await mediator.Publish(
//     new DomainEventNotification<string>("TEST"));

//     return "ok";
// });

            // group.MapPut("/{id:guid}", async (Guid id, UpdateTicketCommand dto, IMediator mediator) =>
            // {
            //     dto = dto with { TicketId = id };
            //     return await mediator.Send(dto);
            // });

            // group.MapGet("/", async ([AsParameters] GetTicketsQuery query, IMediator mediator) =>
            //     await mediator.Send(query));

            // group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            //     await mediator.Send(new GetTicketByIdQuery(id)));

            // group.MapDelete("/{id:guid}", async (Guid id, IResponseFactory responder, IMediator mediator) =>
            // {
            //     await mediator.Send(new SoftDeleteTicketCommand(id));
            //     return responder.Success("Ticket deleted");
            // });
        }
    }

}