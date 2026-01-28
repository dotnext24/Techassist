using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace TechAssistPro.CustomerManagement.Application.Validations
{
    public class CustomerCreateValidator : AbstractValidator<CustomerCreateDto>
{
    public CustomerCreateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").EmailAddress().WithMessage("Invalid email address");
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required").Matches(@"^\d{10}$").WithMessage("Invalid phone number format");
    }
}
}