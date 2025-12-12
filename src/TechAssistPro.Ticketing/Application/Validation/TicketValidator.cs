using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;


namespace TechAssistPro.Ticketing.Application.Validation
{
    
public class TicketValidator : AbstractValidator<CreateTicketDto>
{
    public TicketValidator()
    {       

        RuleFor(t => t.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required.")
            .MaximumLength(100)
            .WithMessage("Customer ID must not exceed 100 characters.");

        RuleFor(t => t.Subject)
            .NotEmpty()
            .WithMessage("Subject is required.")
            .MinimumLength(5)
            .WithMessage("Subject must be at least 5 characters long.")
            .MaximumLength(200)
            .WithMessage("Subject cannot exceed 200 characters.");

        RuleFor(t => t.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MinimumLength(10)
            .WithMessage("Description must be at least 10 characters long.");

        RuleFor(t => t.Category)
            .IsInEnum()
            .WithMessage("Category value is not valid.");

        RuleFor(t => t.Priority)
            .IsInEnum()
            .WithMessage("Priority value is not valid.");

        RuleFor(t => t.Channel)
            .IsInEnum()
            .WithMessage("Channel value is not valid.");
        
    }


}

}